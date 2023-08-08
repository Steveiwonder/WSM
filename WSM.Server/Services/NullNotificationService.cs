namespace WSM.Server.Services
{
    public class NullNotificationService : INotificationService
    {
        public Task SendNotificationAsync(string title, string message)
        {
            return Task.CompletedTask;
        }
    }
}
