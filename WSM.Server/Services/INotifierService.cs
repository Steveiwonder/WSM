namespace WSM.Server.Services
{
    public interface INotificationService
    {

        Task SendNotificationAsync(string title, string message);
    }
}
