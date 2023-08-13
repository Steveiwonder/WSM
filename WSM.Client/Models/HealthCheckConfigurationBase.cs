using System;
using WSM.Shared;

namespace WSM.Client.Models
{
    public abstract class HealthCheckConfigurationBase
    {
        public TimeSpan Interval { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int MissedCheckInLimit = Constants.DefaultMissedCheckInLimit;
        public int BadStatusLimit = Constants.DefaultBadStatusLimit;

    }
}
