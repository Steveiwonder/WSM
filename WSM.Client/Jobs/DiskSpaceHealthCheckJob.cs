using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;

namespace WSM.Client.Jobs
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
            var healthCheckDefinition = GetDefinition<DiskSpaceHealthCheckDefinition>(context);
            try
            {
                var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == healthCheckDefinition.DiskName);
                if (drive == null)
                {
                    await CheckIn(healthCheckDefinition, DiskNotFoundStatus);
                    return;
                }

                if(drive.TotalFreeSpace< healthCheckDefinition.MinimumFreeSpace)
                {
                    string mbAvailable = FormatUtils.SizeSuffix(drive.TotalFreeSpace);
                    await CheckIn(healthCheckDefinition, $"{DiskSpaceLowStatus}, {mbAvailable} available");
                    return;
                }

                await CheckIn(healthCheckDefinition, Constants.AvailableStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
            }
        }
    }
}
