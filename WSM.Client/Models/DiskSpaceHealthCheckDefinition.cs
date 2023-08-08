using System;

namespace WSM.Client.Models
{
    public class DiskSpaceHealthCheckDefinition : HealthCheckDefinitionBase
    {
        public TimeSpan Interval { get; set; }
        public string DiskName { get; set; }
        public long MinimumFreeSpace { get; set; }
    }
}
