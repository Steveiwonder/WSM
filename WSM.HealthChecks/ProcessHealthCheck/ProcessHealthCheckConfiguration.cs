using WSM.Client.Models;

namespace WSM.HealthChecks.ProcessHealthCheck
{
    public class ProcessHealthCheckConfiguration : HealthCheckConfigurationBase
    {
        public string ProcessName { get; set; }
        public int? MinCount { get; set; }
        public int? MaxCount { get; set; }
    }
}
