using WSM.Client.Models;

namespace WSM.HealthChecks.HeartbeatHealthCheck
{
    public class HeartbeatHealthCheckDefinition : HealthCheckDefinitionBase<HeartbeatHealthCheckJob, HeartbeatHealthCheckConfiguration>
    {
        public override string Type => "Heartbeat";
    }
}
