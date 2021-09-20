using System;

namespace Cryptofolio.Collector.Job.IntegrationTests.Data
{
    public abstract class SchedulerTestBase
    {
        protected TimeSpan SchedulerDelay { get; } = TimeSpan.Parse(Environment.GetEnvironmentVariable("SCHEDULER_TEST_DELAY") ?? "00:01:05");
    }
}
