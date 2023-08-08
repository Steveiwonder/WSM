using System;

namespace WSM.Client.Models
{
    public class TcpPortHealthCheckDefinition : IntervalHealthCheckDefinitionBase
    {
        public int Port { get; set; }
        public string Host { get; set; }

    }
}
