using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace WSM.Server.Services
{
    public class WhatsAppNotificationServicerService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppNotificationServicerService> _logger;
        private readonly string _from;
        private readonly string _to;

        public WhatsAppNotificationServicerService(IConfiguration configuration, ILogger<WhatsAppNotificationServicerService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var accountSid = _configuration.GetSection("Twilio:AccountId").Get<string>();
            var authToken = _configuration.GetSection("Twilio:AuthToken").Get<string>();
            _from = _configuration.GetSection("Twilio:From").Get<string>();
            _to = _configuration.GetSection("Twilio:To").Get<string>();
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task SendNotificationAsync(string message)
        {
            try
            {
                message = message.Replace("\r\n", "\n");
                var messageOptions = new CreateMessageOptions(new PhoneNumber(_to));
                messageOptions.From = new PhoneNumber(_from);
                messageOptions.Body = message;

                await MessageResource.CreateAsync(messageOptions);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }
    }
}
