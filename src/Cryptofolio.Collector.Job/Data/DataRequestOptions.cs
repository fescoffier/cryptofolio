namespace Cryptofolio.Collector.Job.Data
{
    /// <summary>
    /// Configuration options 
    /// </summary>
    public class DataRequestOptions
    {
        public int AssetScheduleIntervalMinutes { get; set; }

        public int AssetTickerScheduleIntervalMinutes { get; set; }

        public int ExchangeScheduleIntervalMinutes { get; set; }
    }
}
