using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WSM.Server.Configuration;

namespace WSM.Server.Authentication
{
    public class ApiKeyAuthenticationHandler :
    AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IEnumerable<ServerConfiguration> _servers;

        public ApiKeyAuthenticationHandler
            (IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock,
            IEnumerable<ServerConfiguration> servers)
            : base(options, logger, encoder, clock)
        {
            _servers = servers;
        }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers
                .ContainsKey(Options.ApiKeyHeaderName))
            {
                return Task.FromResult(AuthenticateResult.Fail($"Missing header: {Options.ApiKeyHeaderName}"));
            }

            string apiKey = TryGetApiKey();

            var server = _servers.FirstOrDefault(d => d.ApiKey == apiKey);
            if (server == null)
            {
                return Task.FromResult(AuthenticateResult
                    .Fail($"Invalid token."));
            }
            var claims = new List<Claim>()
            {
                new Claim(ClaimConstants.ApiKey, apiKey),
                new Claim(ClaimConstants.ServerName, server.Name),
            };

            var claimsIdentity = new ClaimsIdentity
                (claims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal
                (claimsIdentity);

            return Task.FromResult(AuthenticateResult.Success
                (new AuthenticationTicket(claimsPrincipal,
                Scheme.Name)));
        }

        private string TryGetApiKey()
        {
            string apiKey = Request.Headers[Options.ApiKeyHeaderName]!;
            if (!string.IsNullOrEmpty(apiKey))
            {
                return apiKey;
            }
            apiKey = Request.Query[Options.ApiKeyQueryStringName];

            return apiKey;
        }
    }
}
