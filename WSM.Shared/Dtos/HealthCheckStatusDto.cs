namespace WSM.Shared.Dtos
{
    public class HealthCheckStatusDto
    {
        public string Name { get; set; }

        public DateTime? LastCheckInTime { get; set; }
        public string LastStatus { get; set; }
        public DateTime NextCheckInTime { get; set; }
        public TimeSpan CheckInInterval { get; set; }
        public int MissedCheckInLimit { get; set; }
        public int BadStatusLimit { get; set; }
        public int BadStatusCount { get; set; }
        public int MissedCheckInCount { get; set; }
        public DateTime? LastMissedCheckInAlertSent { get; set; }
        public DateTime? LastBadStatusAlertSent { get; set; }
        public DateTime NextStatusCheckTime { get; set; }
    }


}
