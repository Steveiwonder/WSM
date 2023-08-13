using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
{
    [DisallowConcurrentExecution]
    public class HeartbeatHealthCheckJob : HealthCheckJobBase
    {
        private readonly ILogger<HeartbeatHealthCheckJob> _logger;

        public HeartbeatHealthCheckJob(ILogger<HeartbeatHealthCheckJob> logger, WSMApiClient apiClient) : base(apiClient)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var healthCheckDefinition = GetDefinition<HeartbeatHealthCheckDefinition>(context);
                await CheckIn(healthCheckDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
