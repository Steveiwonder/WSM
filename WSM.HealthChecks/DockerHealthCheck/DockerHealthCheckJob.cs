using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.HealthChecks.DockerHealthCheck
{
    [DisallowConcurrentExecution]
    public class DockerHealthCheckJob : HealthCheckJobBase
    {
        private const string DockerRunningState = "running";
        private readonly ILogger<DockerHealthCheckJob> _logger;

        public DockerHealthCheckJob(ILogger<DockerHealthCheckJob> logger, WSMApiClient apiClient) : base(apiClient, logger)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            var healthCheckConfiguration = GetConfiguration<DockerContainerHealthCheckConfiguration>(context);
            try
            {
                var client = GetClient();
                var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
                var formattedContainerName = $"/{healthCheckConfiguration.ContainerName}";
                var container = containers.FirstOrDefault(d => d.Names.Contains(formattedContainerName));
                if (container == null)
                {
                    await CheckIn(healthCheckConfiguration, "Container doesn't exist");
                    return;
                }
                if (container.State != DockerRunningState)
                {
                    await CheckIn(healthCheckConfiguration, container.Status);
                    return;
                }
                await CheckIn(healthCheckConfiguration, Constants.AvailableStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }

        private DockerClient GetClient()
        {
            return new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();
        }


    }
}
