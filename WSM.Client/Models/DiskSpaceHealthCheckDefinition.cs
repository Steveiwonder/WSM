using System;

namespace WSM.Client.Models
{
    public class DiskSpaceHealthCheckDefinition : HealthCheckDefinitionBase
    {        
        public string DiskName { get; set; }
        public long MinimumFreeSpace { get; set; }
    }
}
