using MailKit.Net.Smtp;
using MimeKit;

namespace WSM.Server.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IConfiguration configuration, ILogger<EmailNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task SendNotificationAsync(string title, string message)
        {
            try
            {
                var host = _configuration.GetSection("Email:Host").Get<string>();
                var port = _configuration.GetSection("Email:Port").Get<int>();
                var userName = _configuration.GetSection("Email:UserName").Get<string>();
                var password = _configuration.GetSection("Email:Password").Get<string>();
                var from = _configuration.GetSection("Email:From").Get<string>();
                var to = _configuration.GetSection("Email:To").Get<string>();

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(from, from));
                mimeMessage.To.Add(new MailboxAddress(to, to));
                mimeMessage.Subject = title;
                mimeMessage.Body = new TextPart("plain") { Text = message };

                using (var client = new SmtpClient())
                {

                    client.Connect(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(userName, password);
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to send email notification");
            }
            return Task.CompletedTask;
        }
    }
}
