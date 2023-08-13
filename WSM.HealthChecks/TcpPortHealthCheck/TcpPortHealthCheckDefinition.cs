using WSM.Client.Models;

namespace WSM.HealthChecks.TcpPortHealthCheck
{
    public class TcpPortHealthCheckDefinition : HealthCheckDefinitionBase<TcpPortHealthCheckJob, TcpPortHealthCheckConfiguration>
    {
        public override string Type => "Port";
    }
}
