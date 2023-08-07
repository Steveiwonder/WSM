namespace WSM.Server.Dtos
{
    public class HealthCheckStatusDto
    {
        public string Name { get; set;}

        public string LastCheckInTime { get; set; }
        public DateTime NextCheckInTime { get; set; }
        public TimeSpan CheckInInterval { get; set; }
    }

    
}
