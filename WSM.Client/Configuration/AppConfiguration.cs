using System;
using System.Collections.Generic;
using System.Text;
using WSM.Client.Models;

namespace WSM.Client.Configuration
{
    internal class AppConfiguration
    {
        public IEnumerable<HealthCheckConfigurationBase> HealthChecks { get; set; }
        public ServerConfiguration Server { get; set; }
    }
}
