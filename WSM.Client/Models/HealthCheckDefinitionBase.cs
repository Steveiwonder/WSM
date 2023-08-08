using System;
using WSM.Shared;

namespace WSM.Client.Models
{
    public abstract class HealthCheckDefinitionBase
    {
        public TimeSpan Interval { get; set; }
        public string Name { get; set; }
        public HealthCheckType Type { get; set; }
        public int MissedCheckInLimit = Constants.DefaultMissedCheckInLimit;
        public int BadStatusLimit = Constants.DefaultBadStatusLimit;

    }
}
