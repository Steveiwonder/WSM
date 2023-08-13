namespace WSM.Server.Services.Notifications
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string title, string message);
    }
}
