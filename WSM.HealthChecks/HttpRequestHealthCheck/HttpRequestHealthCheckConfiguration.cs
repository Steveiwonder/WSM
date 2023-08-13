using WSM.Client.Models;

namespace WSM.HealthChecks.HttpRequestHealthCheck
{
    public class HttpRequestHealthCheckConfiguration : HealthCheckConfigurationBase
    {
        public string Url { get; set; }
        public HttpMethod Method = HttpMethod.Options;
        public int ExpectedStatusCode { get; set; }
        public TimeSpan? MaxResponseDuration { get; set; }
        public string RequestBody { get; set; }
        public string ExpectedResponseBody { get; set; }
    }
}
