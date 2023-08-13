using WSM.Client.Models;

namespace WSM.HealthChecks.DockerHealthCheck
{
    public class DockerHealthCheckDefinition : HealthCheckDefinitionBase<DockerHealthCheckJob, DockerContainerHealthCheckConfiguration>
    {
        public override string Type => "DockerContainer";
    }
}
