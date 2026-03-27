using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authentication;

namespace WIRM.API.Services
{
    public class AuthService
    {
        private readonly IConfidentialClientApplication _clientApp;
        private readonly ILogger<AuthService> _logger;
        private readonly IMemoryCache _cache;
        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, IMemoryCache cache)
        {
            _clientApp = ConfidentialClientApplicationBuilder
                .Create(configuration["AzureAd:ClientId"])
                .WithClientSecret(configuration["AzureAd:ClientSecret"])
                .WithAuthority(new Uri(configuration["AzureAd:Authority"]))
                .Build();

            _logger = logger;
            _cache = cache;
        }

        private async Task<string> GetAccessTokenOnBehalfOfAsync(string userToken, string username)
        {
            try
            {
                var cacheKey = $"ado_token_{username}";
                if (_cache.TryGetValue(cacheKey, out string cachedToken))
                    return cachedToken;

                var userAssertion = new UserAssertion(userToken);
                var scopes = new[] { "https://app.vssps.visualstudio.com/vso.project", "https://app.vssps.visualstudio.com/vso.work_full" };

                var result = await _clientApp.AcquireTokenOnBehalfOf(scopes, userAssertion).ExecuteAsync();
                _cache.Set(cacheKey, result.AccessToken, TimeSpan.FromMinutes(5));
                return result.AccessToken;
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogError(ex, "User interaction required - consent not granted");
                throw new UnauthorizedAccessException("Admin consent required for Azure DevOps access");
            }
            catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_grant")
            {
                _logger.LogError(ex, "Invalid grant - likely consent issue");
                throw new UnauthorizedAccessException("Permission not granted or token invalid");
            }
            catch (MsalException ex)
            {
                _logger.LogError(ex, "Failed to acquire token on behalf of user");
                throw new UnauthorizedAccessException("Failed to acquire Azure DevOps token", ex);
            }
        }

        public async Task<string> GetAccessToken(IHttpContextAccessor contextAccessor)
        {
            var username = contextAccessor.HttpContext?.User?.Identity?.Name;
            var userToken = await contextAccessor.HttpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(userToken))
                throw new InvalidOperationException("User token is missing");

            var accessToken = await GetAccessTokenOnBehalfOfAsync(userToken, username);
            return accessToken;
        }
    }
}
