using Microsoft.Extensions.Logging;
using Quartz;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.HealthChecks.HeartbeatHealthCheck
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
                var healthCheckConfiguration = GetConfiguration<HeartbeatHealthCheckConfiguration>(context);
                await CheckIn(healthCheckConfiguration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
