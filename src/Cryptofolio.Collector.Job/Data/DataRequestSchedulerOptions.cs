using System.Collections.Generic;

namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Configuration options 
    /// </summary>
    public class DataRequestSchedulerOptions
    {
        /// <summary>
        /// The interval in milliseconds between cron checks of the schedule.
        /// </summary>
        public int CronCheckIntervalMs { get; set; }

        /// <summary>
        /// The interval in milliseconds between cron checks of the schedule while it's invalid.
        /// </summary>
        public int InvalidCronCheckIntervalMs { get; set; }

        /// <summary>
        /// The schedules.
        /// </summary>
        public Dictionary<string, string> Schedules { get; set; }
    }
}
