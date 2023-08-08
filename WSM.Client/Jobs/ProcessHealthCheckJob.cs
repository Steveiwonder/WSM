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
                var status = Process.GetProcessesByName(healthCheckDefinition.ProcessName).Length == 0 ? Constants.NotAvailableStatus : Constants.AvailableStatus;
                await CheckIn(healthCheckDefinition, status);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
