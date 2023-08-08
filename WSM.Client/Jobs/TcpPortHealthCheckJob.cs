using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
{
    [DisallowConcurrentExecution]
    public class TcpPortHealthCheckJob : HealthCheckJobBase
    {

        private readonly ILogger<ProcessHealthCheckJob> _logger;

        public TcpPortHealthCheckJob(ILogger<ProcessHealthCheckJob> logger, WSMApiClient client) : base(client)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var healthCheckDefinition = GetDefinition<TcpPortHealthCheckDefinition>(context);
                string status = GetTcpStatus(healthCheckDefinition);

                await CheckIn(healthCheckDefinition, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }

        private string GetTcpStatus(TcpPortHealthCheckDefinition definition)
        {
            try
            {
                using (TcpClient client = new())
                {
                    client.Connect(GetHost(definition), definition.Port);
                    client.Close();
                }
                return Constants.AvailableStatus;
            }
            catch
            {

            }

            return Constants.NotAvailableStatus;
        }

        private string GetHost(TcpPortHealthCheckDefinition definition)
        {
            return string.IsNullOrEmpty(definition.Host) ? "localhost" : definition.Host;
        }

    }
}
