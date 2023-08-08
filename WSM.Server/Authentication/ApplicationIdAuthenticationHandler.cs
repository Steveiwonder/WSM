using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WSM.Server.Configuration;

namespace WSM.Server.Authentication
{
    public class ApplicationIdAuthenticationHandler :
    AuthenticationHandler<ApplicationIdAuthenticationOptions>
    {
        private readonly IEnumerable<ServerConfiguration> _servers;

        public ApplicationIdAuthenticationHandler
            (IOptionsMonitor<ApplicationIdAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock,
            IEnumerable<ServerConfiguration> servers)
            : base(options, logger, encoder, clock)
        {
            _servers = servers;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers
                .ContainsKey(Options.TokenHeaderName))
            {
                return AuthenticateResult.Fail($"Missing header: {Options.TokenHeaderName}");
            }

            string apiKey = Request
                .Headers[Options.TokenHeaderName]!;

            var server = _servers.FirstOrDefault(d => d.ApiKey == apiKey);
            if (server == null)
            {
                return AuthenticateResult
                    .Fail($"Invalid token.");
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

            return AuthenticateResult.Success
                (new AuthenticationTicket(claimsPrincipal,
                Scheme.Name));
        }
    }
}
