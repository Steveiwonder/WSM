using Microsoft.Extensions.Logging;
using Quartz;
using System.Net.Sockets;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.HealthChecks.TcpPortHealthCheck
{
    [DisallowConcurrentExecution]
    public class TcpPortHealthCheckJob : HealthCheckJobBase
    {

        private readonly ILogger<TcpPortHealthCheckJob> _logger;

        public TcpPortHealthCheckJob(ILogger<TcpPortHealthCheckJob> logger, WSMApiClient apiClient) : base(apiClient, logger)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var healthCheckConfiguration = GetConfiguration<TcpPortHealthCheckConfiguration>(context);
                string status = GetTcpStatus(healthCheckConfiguration);

                await CheckIn(healthCheckConfiguration, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }

        private static string GetTcpStatus(TcpPortHealthCheckConfiguration definition)
        {
            try
            {
                using TcpClient client = new();
                client.Connect(GetHost(definition), definition.Port);
                client.Close();
                return Constants.AvailableStatus;
            }
            catch
            {

            }

            return Constants.NotAvailableStatus;
        }

        private static string GetHost(TcpPortHealthCheckConfiguration definition)
        {
            return string.IsNullOrEmpty(definition.Host) ? "localhost" : definition.Host;
        }

    }
}
