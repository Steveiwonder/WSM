using System.Collections.Concurrent;
using WSM.Server.Configuration;

namespace WSM.Server.Models
{
    internal class RegisteredServer
    {
        internal RegisteredServer(string name)
        {
            Name = name;
            HealthChecks = new ConcurrentDictionary<string, HealthCheckStatus>();
        }

        internal string Name { get; init; }
        internal ConcurrentDictionary<string, HealthCheckStatus> HealthChecks { get; }

        internal bool ContainsHealthCheck(string name)
        {
            return HealthChecks.ContainsKey(name);
        }

        internal HealthCheckStatus GetHealthCheckStatus(string name)
        {
            if (!HealthChecks.ContainsKey(name))
            {
                return null;
            }
            return HealthChecks[name];
        }

        internal bool TryAddHealthCheck(string name, HealthCheckStatus healthCheckStatus)
        {
            return HealthChecks.TryAdd(name, healthCheckStatus);
        }

        internal bool TryRemoveHealthCheck(string name, out HealthCheckStatus healthCheck)
        {
            return HealthChecks.TryRemove(name, out healthCheck);
        }
    }
    internal class HealthCheckStatus
    {
        internal HealthCheck HealthCheck { get; init; }

        internal string Name => HealthCheck.Name;
        internal DateTime NextCheckInTime { get; private set; }
        internal DateTime NextStatusCheckTime { get; private set; }
        internal DateTime? LastCheckInTime { get; private set; }
        internal string LastStatus { get; private set; }
        internal int BadStatusCount { get; private set; }
        internal int MissedCheckInCount { get; private set; }
        internal DateTime? LastBadStatusAlertSent { get; private set; }
        internal DateTime? LastMissedCheckInAlertSent { get; private set; }

        internal void UpdateNextCheckInTime()
        {
            NextCheckInTime = DateTime.UtcNow.Add(HealthCheck.CheckInInterval);
        }

        internal void UpdateLastCheckInTime(DateTime lastCheckIn)
        {
            LastCheckInTime = lastCheckIn;
        }

        internal void UpdateStatus(string status)
        {
            LastStatus = status;
        }

        internal void IncrementBadStatusCount()
        {
            BadStatusCount++;
        }

        internal void IncrementMissedCheckInCount()
        {
            MissedCheckInCount++;
        }

        internal void UpdateLastBadCheckInAlertSent()
        {
            LastBadStatusAlertSent = DateTime.UtcNow;
        }

        internal void UpdateLastMissedCheckInAlertSent()
        {
            LastMissedCheckInAlertSent = DateTime.UtcNow;
        }

        internal void UpdateNextStausCheckTime()
        {
            NextStatusCheckTime = DateTime.UtcNow.Add(HealthCheck.CheckInInterval);
        }
        internal void ResetMissedCheckInCount()
        {
            MissedCheckInCount = 0;
            LastMissedCheckInAlertSent = null;
        }

        internal void ResetBadStatusCount()
        {
            BadStatusCount = 0;
            LastBadStatusAlertSent = null;
        }


    }
}
