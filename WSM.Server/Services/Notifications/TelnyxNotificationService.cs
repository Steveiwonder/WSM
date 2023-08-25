using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Telnyx;
using Microsoft.Extensions.Options;

namespace WSM.Server.Services.Notifications
{
    public class TelnyxNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TelnyxNotificationService> _logger;
        private readonly string _from;
        private readonly string _to;

        public TelnyxNotificationService(IConfiguration configuration, ILogger<TelnyxNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var apiKey = _configuration.GetSection("Telnyx:ApiKey").Get<string>();
            TelnyxConfiguration.SetApiKey(apiKey);
            _from = _configuration.GetSection("Telnyx:From").Get<string>();
            _to = _configuration.GetSection("Telnyx:To").Get<string>();
 
        }

        public async Task SendNotificationAsync(string title, string message)
        {
            try
            {
                MessagingSenderIdService service = new();
                NewMessagingSenderId options = new()
                {
                    From = _from,
                    To = _to,
                    Text = $"{title}\n{message}"
            };
                MessagingSenderId messageResponse = await service.CreateAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }
    }
}
