using System;

namespace WSM.Client.Models
{
    public class DockerContainerHealthCheckDefinition : HealthCheckDefinitionBase
    {
        public TimeSpan Interval { get; set; }
        public string ContainerName { get; set; }
    }
}
