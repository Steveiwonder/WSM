using System;

namespace WSM.Client.Models
{
    public class DockerContainerHealthCheckDefinition : IntervalHealthCheckDefinitionBase
    {        
        public string ContainerName { get; set; }
    }
}
