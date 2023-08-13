using WSM.Client.Models;

namespace WSM.HealthChecks.ProcessHealthCheck
{
    public class ProcessHealthCheckDefinition : HealthCheckDefinitionBase<ProcessHealthCheckJob, ProcessHealthCheckConfiguration>
    {
        public override string Type => "Process";
    }
}
