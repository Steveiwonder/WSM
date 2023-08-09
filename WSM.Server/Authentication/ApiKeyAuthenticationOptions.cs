using Microsoft.AspNetCore.Authentication;

namespace WSM.Server.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string ApiKeyHeaderName { get; set; } = "Authorization";
        public string ApiKeyQueryStringName { get; set; } = "apikey";

    }
}
