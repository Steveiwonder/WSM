using WSM.Server.Services;
using WSM.Shared;
using WSM.Server.Models;
using WSM.Server.Services.Notifications;

namespace WSM.Server.BackgroundServices
{
    public class WSMHealthCheckBackgroundService : BackgroundService
    {

        private readonly WSMHealthCheckService _healthCheckService;
        private readonly ILogger<WSMHealthCheckBackgroundService> _logger;
        private readonly INotificationSender _notificationSender;
        private readonly TimeSpan _alertFrequency;
        private readonly TimeSpan _reportSlipDuration;
        private readonly TimeSpan _backgroundServiceDelay;


        public WSMHealthCheckBackgroundService(WSMHealthCheckService healthCheckService,
            ILogger<WSMHealthCheckBackgroundService> logger,
            IConfiguration configuration,
            INotificationSender notificationSender)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
            _notificationSender = notificationSender;
            _alertFrequency = configuration.GetSection("AlertFrequency").Get<TimeSpan>();
            _reportSlipDuration = configuration.GetSection("ReportSlipDuration").Get<TimeSpan>();
            _backgroundServiceDelay = configuration.GetSection("BackgroundServiceDelay").Get<TimeSpan>();
            if(_backgroundServiceDelay <= TimeSpan.Zero)
            {
                _backgroundServiceDelay = TimeSpan.FromSeconds(15);
            }
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(_backgroundServiceDelay, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Getting server statuses");
                foreach (var serverStatusesKvp in _healthCheckService.GetServerStatuses())
                {
                    _logger.LogInformation($"Processing server {serverStatusesKvp.Key}");
                    foreach (var healthCheckKvp in serverStatusesKvp.Value)
                    {
                        _logger.LogInformation($"Checking health for {healthCheckKvp.Key}");
                        var healthCheckStatus = healthCheckKvp.Value;
                        try
                        {
                            if (healthCheckStatus.NextStatusCheckTime > DateTime.UtcNow)
                            {
                                _logger.LogTrace($"{healthCheckKvp.Key} is not due a health check, next due {healthCheckStatus.NextStatusCheckTime}");
                                continue;
                            }
                            bool hasMissedCheckIn = await HasMissedCheckInAsync(healthCheckStatus);

                            if (hasMissedCheckIn)
                            {

                                healthCheckStatus.IncrementMissedCheckInCount();
                                _logger.LogInformation($"{healthCheckStatus.Name} missed a checkin, {healthCheckStatus.MissedCheckInCount}/{healthCheckStatus.HealthCheck.MissedCheckInLimit}");
                                await SendMissedCheckInAlertAsync(healthCheckStatus);
                            }

                            bool hasBadStatus = await HasBadStatusAsync(healthCheckStatus);
                            if (hasBadStatus)
                            {
                                _logger.LogInformation($"{healthCheckStatus.Name} missed a bad status, {healthCheckStatus.BadStatusCount}/{healthCheckStatus.HealthCheck.BadStatusLimit}");
                                await SendBadStatusAlertAsync(healthCheckStatus);
                            }
                            healthCheckStatus.UpdateNextStausCheckTime();

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ExecuteAsync");
                        }
                    }
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async Task<bool> HasMissedCheckInAsync(HealthCheckStatus healthCheckStatus)
        {
            bool hasMissedCheckIn = DateTime.UtcNow > healthCheckStatus.NextCheckInTime.Add(_reportSlipDuration);

            if (!hasMissedCheckIn && healthCheckStatus.MissedCheckInCount != 0)
            {
                healthCheckStatus.ResetMissedCheckInCount();
                await _notificationSender.SendNotificationAsync($"Checked In {healthCheckStatus.Name}", $"{healthCheckStatus.Name} has checked in");
            }

            return hasMissedCheckIn;
        }

        private async Task<bool> HasBadStatusAsync(HealthCheckStatus healthCheckStatus)
        {
            bool hasBadStatus = healthCheckStatus.LastStatus != Constants.AvailableStatus;

            if (!hasBadStatus && healthCheckStatus.BadStatusCount != 0)
            {
                healthCheckStatus.ResetBadStatusCount();
                await _notificationSender.SendNotificationAsync($"Status OK {healthCheckStatus.Name}", $"{healthCheckStatus.Name} is now OK");

            }

            return hasBadStatus;
        }

        private bool CanSendMissedCheckInAlert(HealthCheckStatus healthCheckStatus)
        {
            bool hasExceededMissedCheckInLimit = healthCheckStatus.MissedCheckInCount >= healthCheckStatus.HealthCheck.MissedCheckInLimit;
            bool hasNeverSentAlert = healthCheckStatus.LastMissedCheckInAlertSent == null;
            bool canSendNextAlert = healthCheckStatus.LastMissedCheckInAlertSent == null || DateTime.UtcNow > healthCheckStatus.LastMissedCheckInAlertSent.Value.Add(_alertFrequency);
            return (hasExceededMissedCheckInLimit) && (hasNeverSentAlert || canSendNextAlert);
        }

        private bool CanSendBadStatusAlert(HealthCheckStatus healthCheckStatus)
        {
            bool hasExceededBadStatusLimit = healthCheckStatus.BadStatusCount >= healthCheckStatus.HealthCheck.BadStatusLimit;
            bool hasNeverSentAlert = healthCheckStatus.LastBadStatusAlertSent == null;
            bool canSendNextAlert = healthCheckStatus.LastBadStatusAlertSent == null || DateTime.UtcNow > healthCheckStatus.LastBadStatusAlertSent.Value.Add(_alertFrequency);
            return (hasExceededBadStatusLimit) && (hasNeverSentAlert || canSendNextAlert);
        }


        private async Task SendMissedCheckInAlertAsync(HealthCheckStatus healthCheckStatus)
        {
            if (!CanSendMissedCheckInAlert(healthCheckStatus))
            {
                return;
            }
            string msg = $"{healthCheckStatus.Name} hasn't checked in, last check in time was {(healthCheckStatus.LastCheckInTime == null ? "never" : healthCheckStatus.LastCheckInTime.ToString())}";
            _logger.LogWarning(msg);

            await _notificationSender.SendNotificationAsync("Missed Check In", msg);
            _logger.LogInformation($"Alert sent for {healthCheckStatus.Name}");
            healthCheckStatus.UpdateLastMissedCheckInAlertSent();
        }

        private async Task SendBadStatusAlertAsync(HealthCheckStatus healthCheckStatus)
        {

            if (!CanSendBadStatusAlert(healthCheckStatus))
            {
                return;
            }
            string msg = $"{healthCheckStatus.Name} reported a bad status, '{healthCheckStatus.LastStatus}', reported {healthCheckStatus.BadStatusCount} times";
            _logger.LogWarning(msg);

            await _notificationSender.SendNotificationAsync("Bad Status Report", msg);
            _logger.LogInformation($"Alert sent for {healthCheckStatus.Name}");
            healthCheckStatus.UpdateLastBadCheckInAlertSent();
        }


    }
}
