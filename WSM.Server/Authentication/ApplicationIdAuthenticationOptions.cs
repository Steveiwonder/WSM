using Microsoft.AspNetCore.Authentication;

namespace WSM.Server.Authentication
{
    public class ApplicationIdAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string TokenHeaderName { get; set; } = "Authorization";

    }
}
