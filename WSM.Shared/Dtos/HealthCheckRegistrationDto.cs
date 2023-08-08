namespace WSM.Shared.Dtos
{
    public class HealthCheckRegistrationDto
    {
        public string Name { get; set; }
        public TimeSpan CheckInInterval { get; set; }
        public int BadStatusLimit { get; set; }
        public int MissedCheckInLimit { get; set; }
    }
}
