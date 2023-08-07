using System;
using System.Collections.Generic;
using System.Text;

namespace WSM.Client.Configuration
{
    public class ServiceConfig
    {
        public const string serviceConfig = "ServiceConfig";
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}
