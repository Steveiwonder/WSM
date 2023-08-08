using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WSM.Shared
{
    public static class Constants
    {
        public static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(5);
        public const string AvailableStatus = "Available";
        public const string NotAvailableStatus = "Unavailable";
        public const int DefaultMissedCheckInLimit = 2;
        public const int DefaultBadStatusLimit = 2;
    }
}
