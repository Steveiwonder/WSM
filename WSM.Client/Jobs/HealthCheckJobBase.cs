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
            await _apiClient.CheckIn(healthCheckDefinition.Name, status ?? Constants.AvailableStatus);
        }
    }
}
