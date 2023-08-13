using Microsoft.Extensions.Logging;
using Quartz;
using System.Diagnostics;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.HealthChecks.ProcessHealthCheck
{
    [DisallowConcurrentExecution]
    public class ProcessHealthCheckJob : HealthCheckJobBase
    {
        private readonly ILogger<ProcessHealthCheckJob> _logger;

        public ProcessHealthCheckJob(ILogger<ProcessHealthCheckJob> logger, WSMApiClient client) : base(client)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var healthCheckConfiguration = GetConfiguration<ProcessHealthCheckConfiguration>(context);
                var minCount = Math.Max(1, healthCheckConfiguration.MinCount ?? 1);
                var maxCount = healthCheckConfiguration.MaxCount;
                var processCount = Process.GetProcessesByName(healthCheckConfiguration.ProcessName).Length;
                string status = Constants.AvailableStatus;
                if (processCount < minCount || maxCount != null && processCount > maxCount)
                {
                    status = $"Found {processCount}, expected between {minCount} - {(maxCount == null ? "No Maximum" : maxCount)}";
                }

                await CheckIn(healthCheckConfiguration, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
