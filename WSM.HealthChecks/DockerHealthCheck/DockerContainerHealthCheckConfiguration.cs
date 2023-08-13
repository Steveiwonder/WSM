using WSM.Client.Models;

namespace WSM.HealthChecks.DockerHealthCheck
{
    public class DockerContainerHealthCheckConfiguration : HealthCheckConfigurationBase
    {
        public string ContainerName { get; set; }
    }
}
