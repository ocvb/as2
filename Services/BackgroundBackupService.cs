using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace _234412H_AS2.Services
{
    public class BackgroundBackupService : IHostedService, IDisposable
    {
        private readonly ILogger<BackgroundBackupService> _logger;
        private readonly IConfiguration _configuration;
        private Timer _timer;
        private readonly DriveService _driveService;
        private readonly string _dbPath;
        private readonly string _backupFolderId;

        public BackgroundBackupService(ILogger<BackgroundBackupService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _dbPath = _configuration.GetConnectionString("DefaultConnection").Replace("Data Source=", "");
            _backupFolderId = _configuration["GoogleDrive:BackupFolderId"];

            var credential = GoogleCredential.FromFile("credentials.json")
                .CreateScoped(DriveService.ScopeConstants.DriveFile);

            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "FreshFarmMarket DB Backup"
            });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Database Backup Service is starting.");

            _timer = new Timer(DoBackup, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(5)); // Backup every 24 hours

            return Task.CompletedTask;
        }

        private async void DoBackup(object state)
        {
            try
            {
                _logger.LogInformation("Starting database backup at: {time}", DateTimeOffset.Now);

                // Verify file exists and is accessible
                if (!File.Exists(_dbPath))
                {
                    throw new FileNotFoundException($"Database file not found at: {_dbPath}");
                }

                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"FreshFarmMarket_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Description = "Automated database backup",
                    Parents = new List<string> { _backupFolderId },
                    MimeType = "application/octet-stream"  // Changed MIME type
                };

                // Use FileShare.ReadWrite to allow concurrent access
                using (var stream = new FileStream(_dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");
                    request.Fields = "id, name, webViewLink";

                    // Add chunk size settings
                    request.ChunkSize = Google.Apis.Upload.ResumableUpload.MinimumChunkSize * 2;

                    request.ProgressChanged += (progress) =>
                    {
                        _logger.LogInformation("Upload progress: Status={status}, Bytes={bytes}",
                            progress.Status,
                            progress.BytesSent);
                    };

                    var response = await request.UploadAsync();

                    if (response.Status == Google.Apis.Upload.UploadStatus.Completed)
                    {
                        _logger.LogInformation("Backup successful - File ID: {fileId}, Link: {link}",
                            request.ResponseBody?.Id,
                            request.ResponseBody?.WebViewLink);
                    }
                    else if (response.Exception != null)
                    {
                        throw new Exception($"Upload failed: {response.Exception.Message}", response.Exception);
                    }
                    else
                    {
                        throw new Exception($"Upload failed with status: {response.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing database backup. Details: {message}", ex.Message);
                if (ex.InnerException != null)
                {
                    _logger.LogError("Inner exception: {inner}", ex.InnerException.Message);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Database Backup Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
