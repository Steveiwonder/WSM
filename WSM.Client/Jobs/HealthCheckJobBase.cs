using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using WSM.Client.Models;
using WSM.Shared;
using WSM.Shared.Dtos;
namespace WSM.Client.Jobs
{
    public abstract class HealthCheckJobBase : IJob
    {
        public const string HealthCheckDataKey = "healthcheck";
        private readonly WSMApiClient _apiClient;
        private readonly ILogger<HealthCheckJobBase> _logger;

        public HealthCheckJobBase(WSMApiClient apiClient, ILogger<HealthCheckJobBase> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public abstract Task Execute(IJobExecutionContext context);

        public static T GetConfiguration<T>(IJobExecutionContext context) where T : class
        {
            return context.JobDetail.JobDataMap[HealthCheckDataKey] as T;
        }

        public async Task CheckIn(HealthCheckConfigurationBase healthCheckConfiguration, string status = null)
        {
            try
            {
                var checkInSuccess = await _apiClient.CheckIn(healthCheckConfiguration.Name, status ?? Constants.AvailableStatus);
                if (!checkInSuccess)
                {
                    await _apiClient.RegisterHealthCheck(new HealthCheckRegistrationDto()
                    {
                        Name = healthCheckConfiguration.Name,
                        BadStatusLimit = healthCheckConfiguration.BadStatusLimit,
                        CheckInInterval = healthCheckConfiguration.Interval,
                        MissedCheckInLimit = healthCheckConfiguration.MissedCheckInLimit
                    });
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error when trying to checkin");
            }

        }
    }
}
