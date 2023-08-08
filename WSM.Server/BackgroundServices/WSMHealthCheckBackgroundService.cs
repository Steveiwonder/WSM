using Twilio.Types;
using Twilio;
using WSM.Server.Services;
using Twilio.Rest.Api.V2010.Account;
using WSM.Shared.Dtos;
using System;
using WSM.Shared;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WSM.Server.Models;

namespace WSM.Server.BackgroundServices
{
    public class WSMHealthCheckBackgroundService : BackgroundService
    {

        private readonly WSMHealthCheckService _healthCheckService;
        private readonly ILogger<WSMHealthCheckBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly TimeSpan _alertFrequency;
        private readonly TimeSpan _reportSlipDuration;
        private readonly TimeSpan _backgroundServiceDelay;


        public WSMHealthCheckBackgroundService(WSMHealthCheckService healthCheckService,
            ILogger<WSMHealthCheckBackgroundService> logger,
            IConfiguration configuration,
            INotificationService notificationService)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
            _configuration = configuration;
            _notificationService = notificationService;
            _alertFrequency = configuration.GetSection("AlertFrequency").Get<TimeSpan>();
            _reportSlipDuration = configuration.GetSection("ReportSlipDuration").Get<TimeSpan>();
            _backgroundServiceDelay = configuration.GetSection("BackgroundServiceDelay").Get<TimeSpan>();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(_backgroundServiceDelay);
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var healthCheck in _healthCheckService.GetStatus())
                {
                    try
                    {
                        if (healthCheck.NextStatusCheckTime > DateTime.UtcNow)
                        {
                            continue;
                        }
                        bool hasMissedCheckIn = HasMissedCheckIn(healthCheck);

                        if (hasMissedCheckIn)
                        {
                            healthCheck.IncrementMissedCheckInCount();
                            SendMissedCheckInAlert(healthCheck);
                        }

                        bool hasBadStatus = HasBadStatus(healthCheck);
                        if (hasBadStatus)
                        {
                            SendBadStatusAlert(healthCheck);
                        }
                        healthCheck.UpdateNextStausCheckTime();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ExecuteAsync");
                    }
                    await Task.Delay(500);
                }
            }
        }

        private bool HasMissedCheckIn(HealthCheckStatus healthCheckStatus)
        {
            bool hasMissedCheckIn = DateTime.UtcNow > healthCheckStatus.NextCheckInTime.Add(_reportSlipDuration);

            if (!hasMissedCheckIn && healthCheckStatus.MissedCheckInCount != 0)
            {
                healthCheckStatus.ResetMissedCheckInCount();
                _notificationService.SendNotificationAsync($"{healthCheckStatus.Name} has checked in");
            }

            return hasMissedCheckIn;
        }

        private bool HasBadStatus(HealthCheckStatus healthCheckStatus)
        {
            bool hasBadStatus = healthCheckStatus.LastStatus != Constants.AvailableStatus;

            if (!hasBadStatus && healthCheckStatus.BadStatusCount != 0)
            {
                healthCheckStatus.ResetBadStatusCount();
                _notificationService.SendNotificationAsync($"{healthCheckStatus.Name} is now OK");

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


        private void SendMissedCheckInAlert(HealthCheckStatus healthCheckStatus)
        {
            if (!CanSendMissedCheckInAlert(healthCheckStatus))
            {
                return;
            }
            string msg = $"{healthCheckStatus.Name} hasn't checked in, last check in time was {(healthCheckStatus.LastCheckInTime == null ? "never" : healthCheckStatus.LastCheckInTime.ToString())}";
            _logger.LogWarning(msg);

            _notificationService.SendNotificationAsync(msg);
            _logger.LogInformation($"Alert sent for {healthCheckStatus.Name}");
            healthCheckStatus.UpdateLastMissedCheckInAlertSent();
        }

        private void SendBadStatusAlert(HealthCheckStatus healthCheckStatus)
        {

            if (!CanSendBadStatusAlert(healthCheckStatus))
            {
                return;
            }
            string msg = $"{healthCheckStatus.Name} reported a bad status, '{healthCheckStatus.LastStatus}', reported {healthCheckStatus.BadStatusCount} times";
            _logger.LogWarning(msg);

            _notificationService.SendNotificationAsync(msg);
            _logger.LogInformation($"Alert sent for {healthCheckStatus.Name}");
            healthCheckStatus.UpdateLastBadCheckInAlertSent();
        }


    }
}
