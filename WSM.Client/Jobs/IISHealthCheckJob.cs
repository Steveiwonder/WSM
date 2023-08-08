using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
{
    [DisallowConcurrentExecution]
    public class IISHealthCheckJob : HealthCheckJobBase
    {

        private readonly ILogger<IISHealthCheckJob> _logger;

        public IISHealthCheckJob(ILogger<IISHealthCheckJob> logger, WSMApiClient client) : base(client)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var healthCheckDefinition = GetDefinition<IISHealthCheckDefinition>(context);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
        private string GetMetabaseRoot(IISHealthCheckDefinition definition)
        {
            return $"IIS://{GetHost(definition)}/W3SVC/";
        }

        private string GetHost(IISHealthCheckDefinition definition)
        {
            return string.IsNullOrEmpty(definition.Host) ? "localhost" : definition.Host;
        }

    }
}
