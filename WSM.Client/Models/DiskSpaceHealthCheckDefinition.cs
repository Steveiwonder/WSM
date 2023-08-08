using System;

namespace WSM.Client.Models
{
    public class DiskSpaceHealthCheckDefinition : IntervalHealthCheckDefinitionBase
    {        
        public string DiskName { get; set; }
        public long MinimumFreeSpace { get; set; }
    }
}
