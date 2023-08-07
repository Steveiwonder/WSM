namespace WSM.Server.Configuration
{
    public class HealthCheck
    {
        public string Name { get; set; }
        public TimeSpan CheckInInterval { get; set; }
    }
}
