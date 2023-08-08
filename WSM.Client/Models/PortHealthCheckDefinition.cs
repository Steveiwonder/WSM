using System;

namespace WSM.Client.Models
{
    public class TcpPortHealthCheckDefinition : HealthCheckDefinitionBase
    {
        public int Port { get; set; }
        public TimeSpan Interval { get; set; }
        public string Host { get; set; }

    }
}
