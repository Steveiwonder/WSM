namespace WSM.Server.Services
{
    public interface INotificationService
    {

        Task SendNotificationAsync(string message);
    }
}
