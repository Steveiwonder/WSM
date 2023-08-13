namespace WSM.Server.Services.Notifications
{
    public interface INotificationSender
    {
        Task SendNotificationAsync(string title, string message);
    }

}
