using System;

namespace WSM.Client.Models
{
    public class DockerContainerHealthCheckDefinition : HealthCheckDefinitionBase
    {        
        public string ContainerName { get; set; }
    }
}
