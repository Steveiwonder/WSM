using WSM.Client.Models;

namespace WSM.HealthChecks.DiskSpaceHealthCheck
{
    public class DiskSpaceHealthCheckDefinition : HealthCheckDefinitionBase<DiskSpaceHealthCheckJob, DiskSpaceHealthCheckConfiguration>
    {
        public override string Type => "DiskSpace";
    }
}
