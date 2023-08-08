using System;
using WSM.Shared;

namespace WSM.Client.Models
{
    public abstract class HealthCheckDefinitionBase
    {
        public string Name { get; set; }
        public HealthCheckType Type { get; set; }
        public int MissedCheckInLimit = Constants.DefaultMissedCheckInLimit;
        public int BadStatusLimit = Constants.DefaultBadStatusLimit;
    }

    public abstract class IntervalHealthCheckDefinitionBase : HealthCheckDefinitionBase
    {
        public TimeSpan Interval { get; set; }
    }
}
