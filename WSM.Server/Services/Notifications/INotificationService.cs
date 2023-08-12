namespace WSM.Server.Services.Notifications
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string title, string message);
    }

    public interface INotificationSender
    {
        Task SendNotificationAsync(string title, string message);
    }

    public class AggregateNotificationSender : INotificationSender
    {
        private readonly ILogger<AggregateNotificationSender> _logger;
        private readonly IEnumerable<INotificationService> _notificationServices;

        public AggregateNotificationSender(ILogger<AggregateNotificationSender> logger,
            IEnumerable<INotificationService> notificationServices)
        {
            _logger = logger;
            _notificationServices = notificationServices;
        }
        public async Task SendNotificationAsync(string title, string message)
        {
            foreach (var notificationService in _notificationServices)
            {
                try
                {
                    await notificationService.SendNotificationAsync(title, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification");
                }
            }
        }
    }

}
