
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Collections.Concurrent;
using WSM.Server.Configuration;
using WSM.Server.Models;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Server.Services
{
    public class WSMHealthCheckService
    {
        private ConcurrentBag<HealthCheck> _healthChecks = new ConcurrentBag<HealthCheck>();
        private ConcurrentDictionary<string, HealthCheckStatus> _healthCheckStatus = new ConcurrentDictionary<string, HealthCheckStatus>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<WSMHealthCheckService> _logger;
        private readonly INotificationService _notificationService;

        public WSMHealthCheckService(ILogger<WSMHealthCheckService> logger, INotificationService notificationService)
        {

            _logger = logger;
            _notificationService = notificationService;
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
            if (healthCheckReport.Status != Constants.AvailableStatus)
            {
                healthCheckStatus.IncrementBadStatusCount();
            }
        }

        internal IEnumerable<HealthCheckStatus> GetStatus()
        {
            return _healthCheckStatus.Values;
        }

        internal void Register(HealthCheckRegistrationsDto healthCheckRegistrations)
        {
            _healthChecks.Clear();
            _healthCheckStatus.Clear();
            foreach (var registration in healthCheckRegistrations.Registrations)
            {
                var healthCheck = new HealthCheck()
                {
                    Name = registration.Name,
                    CheckInInterval = registration.CheckInInterval,
                    BadStatusLimit = registration.BadStatusLimit,
                    MissedCheckInLimit = registration.MissedCheckInLimit
                };

                _healthChecks.Add(healthCheck);

                _notificationService.SendNotificationAsync($"New Health Check Registration\r\nName: {registration.Name}\r\nInterval: {registration.CheckInInterval}\r\nMissed Limit: {registration.MissedCheckInLimit}\r\nBad Status Limit: {registration.BadStatusLimit}");
            }

            foreach (var healthCheck in _healthChecks)
            {
                var healthCheckStatus = new HealthCheckStatus()
                {
                    HealthCheck = healthCheck
                };
                healthCheckStatus.UpdateNextCheckInTime();
                _healthCheckStatus.AddOrUpdate(healthCheck.Name, healthCheckStatus, (key, hc) => hc);
            }
        }
    }
}
