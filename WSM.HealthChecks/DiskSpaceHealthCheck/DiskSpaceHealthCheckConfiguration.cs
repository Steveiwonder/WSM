using WSM.Client.Models;

namespace WSM.HealthChecks.DiskSpaceHealthCheck
{
    public class DiskSpaceHealthCheckConfiguration : HealthCheckConfigurationBase
    {
        public string DiskName { get; set; }
        public long MinimumFreeSpace { get; set; }
    }
}
