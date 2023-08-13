using WSM.Client.Models;
using WSM.HealthChecks.FreeMemoryHealthCheck;

namespace WSM.PluginExample
{
    public class FreeMemoryHealthCheckDefinition : HealthCheckDefinitionBase<FreeMemoryHealthCheckJob, FreeMemoryHealthCheckConfiguration>
    {
        public override string Type => "FreeMemory";
    }
}
