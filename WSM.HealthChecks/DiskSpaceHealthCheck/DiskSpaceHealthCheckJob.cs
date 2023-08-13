using Microsoft.Extensions.Logging;
using Quartz;
using WSM.Client.Jobs;
using WSM.Shared;

namespace WSM.HealthChecks.DiskSpaceHealthCheck
{
    [DisallowConcurrentExecution]
    public class DiskSpaceHealthCheckJob : HealthCheckJobBase
    {
        private readonly ILogger<DiskSpaceHealthCheckJob> _logger;
        private const string DiskNotFoundStatus = "Disk not found";
        private const string DiskSpaceLowStatus = "Disk space low";
        public DiskSpaceHealthCheckJob(ILogger<DiskSpaceHealthCheckJob> logger, WSMApiClient client) : base(client)
        {
            _logger = logger;
        }
        public override async Task Execute(IJobExecutionContext context)
        {
            var healthCheckConfiguration = GetConfiguration<DiskSpaceHealthCheckConfiguration>(context);
            try
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == healthCheckConfiguration.DiskName);
                if (drive == null)
                {
                    await CheckIn(healthCheckConfiguration, DiskNotFoundStatus);
                    return;
                }

                if (drive.TotalFreeSpace < healthCheckConfiguration.MinimumFreeSpace)
                {
                    string mbAvailable = FormatUtils.SizeSuffix(drive.TotalFreeSpace);
                    await CheckIn(healthCheckConfiguration, $"{DiskSpaceLowStatus}, {mbAvailable} available");
                    return;
                }

                await CheckIn(healthCheckConfiguration, Constants.AvailableStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
