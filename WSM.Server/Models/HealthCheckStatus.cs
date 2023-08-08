using WSM.Server.Configuration;

namespace WSM.Server.Models
{
    internal class HealthCheckStatus
    {
        public HealthCheck HealthCheck { get; init; }

        public string Name => HealthCheck.Name;
        public DateTime NextCheckInTime { get; private set; }
        public DateTime NextStatusCheckTime { get; private set; }
        public DateTime? LastCheckInTime { get; private set; }
        public string LastStatus { get; private set; }
        public int BadStatusCount { get; private set; }
        public int MissedCheckInCount { get; private set; }
        public DateTime? LastBadStatusAlertSent { get; private set; }
        public DateTime? LastMissedCheckInAlertSent { get; private set; }

        public void UpdateNextCheckInTime()
        {
            NextCheckInTime = DateTime.UtcNow.Add(HealthCheck.CheckInInterval);
        }

        public void UpdateLastCheckInTime(DateTime lastCheckIn)
        {
            LastCheckInTime = lastCheckIn;
        }

        public void UpdateStatus(string status)
        {
            LastStatus = status;
        }

        public void IncrementBadStatusCount()
        {
            BadStatusCount++;
        }

        public void IncrementMissedCheckInCount()
        {
            MissedCheckInCount++;
        }

        public void UpdateLastBadCheckInAlertSent()
        {
            LastBadStatusAlertSent = DateTime.UtcNow;
        }

        public void UpdateLastMissedCheckInAlertSent()
        {
            LastMissedCheckInAlertSent = DateTime.UtcNow;
        }

        public void UpdateNextStausCheckTime()
        {
            NextStatusCheckTime = DateTime.UtcNow.Add(HealthCheck.CheckInInterval);
        }
        public void ResetMissedCheckInCount()
        {
            MissedCheckInCount = 0;
            LastMissedCheckInAlertSent = null;
        }

        public void ResetBadStatusCount()
        {
            BadStatusCount = 0;
            LastBadStatusAlertSent = null;
        }

    
    }
}
