using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace WSM.Server.Services
{
    public class WhatsAppNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppNotificationService> _logger;
        private readonly string _from;
        private readonly string _to;

        public WhatsAppNotificationService(IConfiguration configuration, ILogger<WhatsAppNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            var accountSid = _configuration.GetSection("Twilio:AccountId").Get<string>();
            var authToken = _configuration.GetSection("Twilio:AuthToken").Get<string>();
            _from = _configuration.GetSection("Twilio:From").Get<string>();
            _to = _configuration.GetSection("Twilio:To").Get<string>();
            TwilioClient.Init(accountSid, authToken);
        }

        public async Task SendNotificationAsync(string title, string message)
        {
            try
            {
                message = message.Replace("\r\n", "\n");
                var messageOptions = new CreateMessageOptions(new PhoneNumber(_to));
                messageOptions.From = new PhoneNumber(_from);
                messageOptions.Body = $"{title}\n{message}";

               var resource =  await MessageResource.CreateAsync(messageOptions);
                if(resource != null) { }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
            }
        }
    }
}
