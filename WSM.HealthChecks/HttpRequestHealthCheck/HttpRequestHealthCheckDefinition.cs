using WSM.Client.Models;

namespace WSM.HealthChecks.HttpRequestHealthCheck
{
    public class HttpRequestHealthCheckDefinition : HealthCheckDefinitionBase<HttpRequestHealthCheckJob, HttpRequestHealthCheckConfiguration>
    {
        public override string Type => "Http";
    }
}
