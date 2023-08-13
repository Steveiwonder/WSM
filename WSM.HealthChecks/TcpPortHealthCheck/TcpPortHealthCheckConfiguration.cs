using System;
using WSM.Client.Models;

namespace WSM.HealthChecks.TcpPortHealthCheck
{
    public class TcpPortHealthCheckConfiguration : HealthCheckConfigurationBase
    {
        public int Port { get; set; }
        public string Host { get; set; }

    }
}
