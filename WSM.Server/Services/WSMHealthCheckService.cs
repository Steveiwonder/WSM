using System.Collections.Concurrent;
using WSM.Server.Configuration;
using WSM.Server.Models;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Server.Services
{
    public class WSMHealthCheckService
    {
        private ConcurrentDictionary<string, HealthCheck> _healthChecks = new ConcurrentDictionary<string, HealthCheck>();
        private ConcurrentDictionary<string, HealthCheckStatus> _healthCheckStatus = new ConcurrentDictionary<string, HealthCheckStatus>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<WSMHealthCheckService> _logger;
        private readonly INotificationService _notificationService;

        public WSMHealthCheckService(ILogger<WSMHealthCheckService> logger, INotificationService notificationService)
        {

            _logger = logger;
            _notificationService = notificationService;
        }


        public bool CheckIn(HealthCheckReportDto healthCheckReport)
        {
            if (!_healthCheckStatus.ContainsKey(healthCheckReport.Name))
            {
                _logger.LogWarning($"Unknown health check name {healthCheckReport.Name}");
                return false;
            }
            _logger.LogInformation($"Check in from {healthCheckReport.Name}");
            var healthCheckStatus = _healthCheckStatus[healthCheckReport.Name];
            healthCheckStatus.UpdateNextCheckInTime();
            healthCheckStatus.UpdateLastCheckInTime(DateTime.UtcNow);
            healthCheckStatus.UpdateStatus(healthCheckReport.Status);
            if (healthCheckReport.Status != Constants.AvailableStatus)
            {
                healthCheckStatus.IncrementBadStatusCount();
            }
            return true;
        }

        internal IEnumerable<HealthCheckStatus> GetStatus()
        {
            return _healthCheckStatus.Values;
        }     
        internal void Register(HealthCheckRegistrationDto registration)
        {
            _healthChecks.TryRemove(registration.Name, out _);
            _healthCheckStatus.TryRemove(registration.Name, out _);

            var healthCheck = new HealthCheck()
            {
                Name = registration.Name,
                CheckInInterval = registration.CheckInInterval,
                BadStatusLimit = registration.BadStatusLimit,
                MissedCheckInLimit = registration.MissedCheckInLimit
            };

            var healthCheckStatus = new HealthCheckStatus()
            {
                HealthCheck = healthCheck
            };
            healthCheckStatus.UpdateNextCheckInTime();
            _healthChecks.TryAdd(healthCheck.Name, healthCheck);
            _healthCheckStatus.TryAdd(healthCheck.Name, healthCheckStatus);
            _notificationService.SendNotificationAsync($"New Health Check Registration\r\nName: {registration.Name}\r\nInterval: {registration.CheckInInterval}\r\nMissed Limit: {registration.MissedCheckInLimit}\r\nBad Status Limit: {registration.BadStatusLimit}");

        }
    }


}
