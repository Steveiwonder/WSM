using WSM.Client.Models;

namespace WSM.PluginExample
{
    public class FreeMemoryHealthCheckConfiguration: HealthCheckConfigurationBase
    {
        public long MinimumFreeMemory { get; set; }
    }
}
