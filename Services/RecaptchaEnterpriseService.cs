using Google.Api.Gax.ResourceNames;
using Google.Cloud.RecaptchaEnterprise.V1;
using Microsoft.Extensions.Configuration;

namespace _234412H_AS2.Services
{
    public class RecaptchaEnterpriseService : IRecaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly RecaptchaEnterpriseServiceClient _client;
        private readonly string _projectId;
        private readonly string _siteKey;

        public RecaptchaEnterpriseService(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = RecaptchaEnterpriseServiceClient.Create();
            _projectId = "precise-crowbar-364909"; // Your Google Cloud project ID
            _siteKey = _configuration["Recaptcha:SiteKey"];
        }

        public async Task<bool> VerifyToken(string token)
        {
            try
            {
                var projectName = new ProjectName(_projectId);
                var request = new CreateAssessmentRequest
                {
                    Assessment = new Assessment
                    {
                        Event = new Event
                        {
                            SiteKey = _siteKey,
                            Token = token,
                            ExpectedAction = "login"
                        }
                    },
                    ParentAsProjectName = projectName
                };

                var response = await _client.CreateAssessmentAsync(request);

                if (!response.TokenProperties.Valid)
                {
                    Console.WriteLine($"Token invalid: {response.TokenProperties.InvalidReason}");
                    return false;
                }

                if (response.TokenProperties.Action != "login")
                {
                    Console.WriteLine($"Action mismatch: {response.TokenProperties.Action}");
                    return false;
                }

                // Consider scores >= 0.5 as legitimate attempts
                return response.RiskAnalysis.Score >= 0.5f;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"reCAPTCHA verification failed: {ex.Message}");
                return false;
            }
        }
    }
}
