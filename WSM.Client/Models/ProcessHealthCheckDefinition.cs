using System;

namespace WSM.Client.Models
{
    public class ProcessHealthCheckDefinition : IntervalHealthCheckDefinitionBase
    {
        public string ProcessName { get; set; }
    }
}
