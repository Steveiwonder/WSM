using Microsoft.Extensions.Logging;
using Quartz;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.PluginExample
{
    public class MyFirstHealthCheckJob : HealthCheckJobBase
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger<MyFirstHealthCheckJob> _logger;
#pragma warning restore IDE0052 // Remove unread private members

        public MyFirstHealthCheckJob(ILogger<MyFirstHealthCheckJob> logger, WSMApiClient apiClient) : base(apiClient, logger)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            var healthCheckConfiguration = GetConfiguration<MyFirstHealthCheckConfiguration>(context);

            Console.WriteLine($"I'm doing really important work with my important value {healthCheckConfiguration.ReallyImportantValue}");

            await Task.CompletedTask;
        }
    }
}
