
using WSM.Server.Configuration;
using WSM.Server.Models;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Server.Services
{
    public class WSMHealthCheckService
    {
        private readonly IEnumerable<HealthCheck> _healthChecks;
        private readonly ILogger<WSMHealthCheckService> _logger;
        private readonly Dictionary<string, HealthCheckStatus> _healthCheckStatus;
        public WSMHealthCheckService(IEnumerable<HealthCheck> healthChecks, ILogger<WSMHealthCheckService> logger)
        {
            _healthChecks = healthChecks;
            _logger = logger;
            _healthCheckStatus = _healthChecks.ToDictionary(d => d.Name, v =>
            {
                var healthCheckStatus = new HealthCheckStatus()
                {
                    HealthCheck = v,
                };
                healthCheckStatus.UpdateNextCheckInTime();

                return healthCheckStatus;
            }, StringComparer.OrdinalIgnoreCase);
        }


        public void CheckIn(HealthCheckReportDto healthCheckReport)
        {
            if (!_healthCheckStatus.ContainsKey(healthCheckReport.Name))
            {
                _logger.LogWarning($"Unknown health check name {healthCheckReport.Name}");
                return;
            }
            _logger.LogInformation($"Check in from {healthCheckReport.Name}");
            var healthCheckStatus = _healthCheckStatus[healthCheckReport.Name];
            healthCheckStatus.UpdateNextCheckInTime();
            healthCheckStatus.UpdateLastCheckInTime(DateTime.UtcNow);
            healthCheckStatus.UpdateStatus(healthCheckReport.Status);
            if(healthCheckReport.Status != Constants.AvailableStatus)
            {
                healthCheckStatus.IncrementBadStatusCount();
            }
        }

        internal IEnumerable<HealthCheckStatus> GetStatus()
        {
            return _healthCheckStatus.Values;
        }

    }
}
