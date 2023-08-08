using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
{
    [DisallowConcurrentExecution]
    public class DockerHealthCheckJob : HealthCheckJobBase
    {
        private const string DockerRunningState = "running";
        private readonly ILogger<ProcessHealthCheckJob> _logger;

        public DockerHealthCheckJob(ILogger<DockerHealthCheckJob> logger, WSMApiClient client) : base(client)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            var healthCheckDefinition = GetDefinition<DockerContainerHealthCheckDefinition>(context);
            try
            {
                var client = GetClient();
                var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
                var formattedContainerName = $"/{healthCheckDefinition.ContainerName}";
                var container = containers.FirstOrDefault(d => d.Names.Contains(formattedContainerName));
                if (container == null)
                {
                    await CheckIn(healthCheckDefinition, "Container doesn't exist");
                    return;
                }
                if (container.State != DockerRunningState)
                {
                    await CheckIn(healthCheckDefinition, container.Status);
                    return;
                }
                await CheckIn(healthCheckDefinition, Constants.AvailableStatus);
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
