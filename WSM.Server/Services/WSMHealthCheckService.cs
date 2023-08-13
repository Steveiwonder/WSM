using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using WSM.Server.Configuration;
using WSM.Server.Models;
using WSM.Server.Services.Notifications;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Server.Services
{
    public class WSMHealthCheckService
    {

        private ConcurrentDictionary<string, RegisteredServer> _servers =
            new ConcurrentDictionary<string, RegisteredServer>(StringComparer.OrdinalIgnoreCase);


        private readonly ILogger<WSMHealthCheckService> _logger;
        private readonly INotificationSender _notificationSender;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal ClaimsPrincipal => _httpContextAccessor?.HttpContext?.User;

        public WSMHealthCheckService(ILogger<WSMHealthCheckService> logger, INotificationSender notificationSender, IHttpContextAccessor httpContextAccessor)
        {

            _logger = logger;
            _notificationSender = notificationSender;
            _httpContextAccessor = httpContextAccessor;
        }



        private RegisteredServer GetServer()
        {
            var apiKey = ClaimsPrincipal.FindFirstValue(ClaimConstants.ApiKey);
            var serverName = ClaimsPrincipal.FindFirstValue(ClaimConstants.ServerName);
            if (!_servers.ContainsKey(apiKey))
            {
                var server = new RegisteredServer(serverName);
                _servers.TryAdd(apiKey, server);
                return server;
            }
            return _servers[apiKey];
        }


        public bool CheckIn(HealthCheckReportDto healthCheckReport)
        {
            var server = GetServer();
            _logger.LogInformation($"CheckIn from {server.Name}.{healthCheckReport.Name}");
            if (!server.ContainsHealthCheck(healthCheckReport.Name))
            {
                _logger.LogWarning($"Unknown health check name {server.Name}.{healthCheckReport.Name}");
                return false;
            }
            _logger.LogInformation($"Check in from {server.Name}.{healthCheckReport.Name}");
            var healthCheckStatus = server.GetHealthCheckStatus(healthCheckReport.Name);
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
            return GetServer().HealthChecks.Values;
        }

        internal Dictionary<string, Dictionary<string, HealthCheckStatus>> GetServerStatuses()
        {
            return _servers.ToDictionary(key => key.Key, value =>
            {
                return new Dictionary<string, HealthCheckStatus>(value.Value.HealthChecks);
            });
        }

        internal void Register(HealthCheckRegistrationDto registration)
        {
            var server = GetServer();

            _logger.LogInformation($"Registering new health {server.Name}.{registration.Name}");
            server.TryRemoveHealthCheck(registration.Name, out _);
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
            server.TryAddHealthCheck(healthCheck.Name, healthCheckStatus);
            _notificationSender.SendNotificationAsync("New Health Check Registration", $"Server: {server.Name}\r\nName: {registration.Name}\r\nInterval: {registration.CheckInInterval}\r\nMissed Limit: {registration.MissedCheckInLimit}\r\nBad Status Limit: {registration.BadStatusLimit}");

        }

        internal void ClearHealthChecks()
        {
            var server = GetServer();
            _logger.LogInformation($"Clearing health checks for {server.Name}, {server.HealthChecks.Count} will be removed");
            server.HealthChecks.Clear();
        }
    }


}
