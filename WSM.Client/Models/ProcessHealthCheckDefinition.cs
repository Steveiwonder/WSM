using System;

namespace WSM.Client.Models
{
    public class ProcessHealthCheckDefinition : HealthCheckDefinitionBase
    {
        public string ProcessName { get; set; }
        public int? MinCount { get; set; }
        public int? MaxCount { get; set; }
    }
}
