using System;
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
        /// The schedules.
        /// </summary>
        public Dictionary<string, string> Schedules { get; set; }

        /// <summary>
        /// The Redis key to store the schedulers list.
        /// </summary>
        public string SchedulersLockKey { get; set; }

        /// <summary>
        /// The timespan to set the schedulers lock expiration.
        /// </summary>
        public TimeSpan SchedulersLockExpiration { get; set; }

        /// <summary>
        /// The timespan that defines the interval between two schedulers lock checks.
        /// </summary>
        public TimeSpan SchedulersLockCheckInterval { get; set; }

        /// <summary>
        /// The Redis key to store the schedulers list.
        /// </summary>
        public string SchedulersHashKey { get; set; }

        /// <summary>
        /// The Redis key format to store the schedules hash.
        /// </summary>
        public string SchedulesHashKeyFormat { get; set; }
    }
}
