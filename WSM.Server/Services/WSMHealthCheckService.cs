using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using WSM.Server.Configuration;
using WSM.Server.Models;
using WSM.Shared;
using WSM.Shared.Dtos;

namespace WSM.Server.Services
{
    public class WSMHealthCheckService
    {

        private ConcurrentDictionary<string, RegisteredServer> _servers =
            new ConcurrentDictionary<string, RegisteredServer>(StringComparer.OrdinalIgnoreCase);


        private readonly ILogger<WSMHealthCheckService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ClaimsPrincipal ClaimsPrincipal => _httpContextAccessor?.HttpContext?.User;

        public WSMHealthCheckService(ILogger<WSMHealthCheckService> logger, INotificationService notificationService, IHttpContextAccessor httpContextAccessor)
        {

            _logger = logger;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
        }



        private RegisteredServer GetServer()
        {
            var applicationId = ClaimsPrincipal.FindFirstValue(ClaimConstants.ApiKey);
            var serverName = ClaimsPrincipal.FindFirstValue(ClaimConstants.ServerName);
            if (!_servers.ContainsKey(applicationId))
            {
                var server = new RegisteredServer(serverName);
                _servers.TryAdd(applicationId, server);
                return server;
            }
            return _servers[applicationId];
        }


        public bool CheckIn(HealthCheckReportDto healthCheckReport)
        {
            var server = GetServer();
            if (!server.ContainsHealthCheck(healthCheckReport.Name))
            {
                _logger.LogWarning($"Unknown health check name {healthCheckReport.Name}");
                return false;
            }
            _logger.LogInformation($"Check in from {healthCheckReport.Name}");
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
            _notificationService.SendNotificationAsync("New Health Check Registration", $"Server: {server.Name}\r\nName: {registration.Name}\r\nInterval: {registration.CheckInInterval}\r\nMissed Limit: {registration.MissedCheckInLimit}\r\nBad Status Limit: {registration.BadStatusLimit}");

        }
    }


}
