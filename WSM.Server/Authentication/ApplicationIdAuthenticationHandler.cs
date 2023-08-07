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
        private readonly ApplicationIds _applicationIds;

        public ApplicationIdAuthenticationHandler
            (IOptionsMonitor<ApplicationIdAuthenticationOptions> options,
            ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock,
            ApplicationIds applicationIds)
            : base(options, logger, encoder, clock)
        {
            _applicationIds = applicationIds;
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            //check header first
            if (!Request.Headers
                .ContainsKey(Options.TokenHeaderName))
            {
                return AuthenticateResult.Fail($"Missing header: {Options.TokenHeaderName}");
            }

            //get the header and validate
            string token = Request
                .Headers[Options.TokenHeaderName]!;

            //usually, this is where you decrypt a token and/or lookup a database.
            if (!_applicationIds.Values.Contains(token))
            {
                return AuthenticateResult
                    .Fail($"Invalid token.");
            }
            //Success! Add details here that identifies the user
            var claims = new List<Claim>()
        {
            new Claim("ApplicationId", token)
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
