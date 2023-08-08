using WSM.Shared;

namespace WSM.Server.Configuration
{
    public class HealthCheck
    {
        public string Name { get; set; }
        public TimeSpan CheckInInterval { get; set; }
        public int MissedCheckInLimit { get; set; } = Constants.DefaultMissedCheckInLimit;
        public int BadStatusLimit { get; set; } = Constants.DefaultBadStatusLimit;
    }
}
