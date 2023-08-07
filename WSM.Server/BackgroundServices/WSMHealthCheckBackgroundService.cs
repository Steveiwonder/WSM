using Twilio.Types;
using Twilio;
using WSM.Server.Dtos;
using WSM.Server.Services;
using Twilio.Rest.Api.V2010.Account;

namespace WSM.Server.BackgroundServices
{
    public class WSMHealthCheckBackgroundService : BackgroundService
    {
        private readonly WSMHealthCheckService _healthCheckService;
        private readonly ILogger<WSMHealthCheckBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, DateTime> _alertHistory = new Dictionary<string, DateTime>();
        private readonly TimeSpan _alertFrequency;


        public WSMHealthCheckBackgroundService(WSMHealthCheckService healthCheckService,
            ILogger<WSMHealthCheckBackgroundService> logger,
            IConfiguration configuration)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
            _configuration = configuration;
            _alertFrequency = configuration.GetSection("AlertFrequency").Get<TimeSpan>();
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var healthCheck in _healthCheckService.GetStatus())
                {
                    try
                    {
                        if (HasMissedCheckIn(healthCheck))
                        {
                            SendAlert(healthCheck);
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex, "ExecuteAsync");
                    }
                    await Task.Delay(500);
                }
            }
        }

        private bool HasMissedCheckIn(HealthCheckStatusDto healthCheckStatus)
        {
            return DateTime.UtcNow > healthCheckStatus.NextCheckInTime;
        }

        private bool CanSendAlert(string healthCheckName)
        {
            if (!_alertHistory.ContainsKey(healthCheckName))
            {
                return true;
            }
            return _alertHistory[healthCheckName].Add(_alertFrequency) < DateTime.UtcNow;
        }
        private void SendAlert(HealthCheckStatusDto healthCheckStatus)
        {
            if (!CanSendAlert(healthCheckStatus.Name))
            {
                return;
            }
            string msg = $"{healthCheckStatus.Name} hasn't checked in, last check in time was {(healthCheckStatus.LastCheckInTime == null ? "never" : healthCheckStatus.LastCheckInTime.ToString())}";
            _logger.LogWarning(msg);

            var accountSid = _configuration.GetSection("Twilio:AccountId").Get<string>();
            var authToken = _configuration.GetSection("Twilio:AuthToken").Get<string>();
            var from = _configuration.GetSection("Twilio:From").Get<string>();
            var to = _configuration.GetSection("Twilio:To").Get<string>();
            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber(to));
            messageOptions.From = new PhoneNumber(from);
            messageOptions.Body = msg;


            var message = MessageResource.Create(messageOptions);
            Console.WriteLine(message.Body);


            if (_alertHistory.ContainsKey(healthCheckStatus.Name))
            {
                _alertHistory[healthCheckStatus.Name] = DateTime.UtcNow;
            }
            else
            {
                _alertHistory.Add(healthCheckStatus.Name, DateTime.UtcNow);
            }
        }
    }
}
