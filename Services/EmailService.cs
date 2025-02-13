using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using _234412H_AS2.Services.Interfaces;

namespace _234412H_AS2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var emailConfig = _configuration.GetSection("EmailSettings");
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(emailConfig["SenderName"], emailConfig["SenderEmail"]));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = body
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    emailConfig["SmtpServer"],
                    int.Parse(emailConfig["SmtpPort"]),
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(emailConfig["SenderEmail"], emailConfig["Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} - Error: {Error}", to, ex.Message);
                throw;
            }
        }
    }
}
