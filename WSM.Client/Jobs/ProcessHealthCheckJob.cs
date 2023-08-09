using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
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
                var healthCheckDefinition = GetDefinition<ProcessHealthCheckDefinition>(context);
                var minCount = Math.Max(1, healthCheckDefinition.MinCount ?? 1);
                var maxCount = healthCheckDefinition.MaxCount;
                var processCount = Process.GetProcessesByName(healthCheckDefinition.ProcessName).Length;
                string status = Constants.AvailableStatus;
                if (processCount < minCount || maxCount != null && processCount > maxCount)
                {
                    status = $"Found {processCount}, expected between {minCount} - {(maxCount == null ? "No Maximum" : maxCount)}";
                }
                
                await CheckIn(healthCheckDefinition, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
