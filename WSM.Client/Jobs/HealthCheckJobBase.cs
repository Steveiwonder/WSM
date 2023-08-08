using Quartz;
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

        public HealthCheckJobBase(WSMApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public abstract Task Execute(IJobExecutionContext context);

        public T GetDefinition<T>(IJobExecutionContext context) where T : class
        {
            return context.JobDetail.JobDataMap[HealthCheckDataKey] as T;
        }

        public async Task CheckIn(HealthCheckDefinitionBase healthCheckDefinition, string status = null)
        {
            var checkInSuccess = await _apiClient.CheckIn(healthCheckDefinition.Name, status ?? Constants.AvailableStatus);
            if (!checkInSuccess)
            {
                await _apiClient.RegisterHealthCheck(new HealthCheckRegistrationDto()
                {
                    Name = healthCheckDefinition.Name,
                    BadStatusLimit = healthCheckDefinition.BadStatusLimit,
                    CheckInInterval = healthCheckDefinition.Interval,
                    MissedCheckInLimit = healthCheckDefinition.MissedCheckInLimit
                });
            }

        }
    }
}
