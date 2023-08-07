using WSM.Server.Configuration;
using WSM.Server.Dtos;

namespace WSM.Server.Services
{
    public class WSMHealthCheckService
    {
        class HealthCheckStatus
        {
            public HealthCheck HealthCheck { get; init; }
            public DateTime NextCheckInTime { get; private set; }
            public DateTime? LastCheckInTime { get; private set; }

            public void UpdateNextCheckInTime()
            {
                NextCheckInTime = DateTime.UtcNow.Add(HealthCheck.CheckInInterval);
            }

            public void UpdateLastCheckInTime(DateTime lastCheckIn)
            {
                LastCheckInTime = lastCheckIn;
            }

        }

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


        public void CheckIn(string healthCheckName)
        {
            if (!_healthCheckStatus.ContainsKey(healthCheckName))
            {
                _logger.LogWarning($"Unknown health check name {healthCheckName}");
                return;
            }
            _logger.LogInformation($"Check in from {healthCheckName}");
            var healthCheckStatus = _healthCheckStatus[healthCheckName]; 
            healthCheckStatus.UpdateNextCheckInTime();
            healthCheckStatus.UpdateLastCheckInTime(DateTime.UtcNow);
        }

        public IEnumerable<HealthCheckStatusDto> GetStatus()
        {
            return _healthCheckStatus.Select(d =>
            {

                return new HealthCheckStatusDto()
                {
                    Name = d.Key,
                    CheckInInterval = d.Value.HealthCheck.CheckInInterval,
                    LastCheckInTime = d.Value.LastCheckInTime == null ? "never" : d.Value.LastCheckInTime.Value.ToString("u"),
                    NextCheckInTime = d.Value.NextCheckInTime,
                };
            });
        }
    }
}
