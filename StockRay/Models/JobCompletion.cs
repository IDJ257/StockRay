namespace StockRay.Models
{
    // One row per job. This is the scheduled time, not the time it finished.
    public class JobCompletion
    {
        public string JobName { get; set; } = string.Empty;

        public DateTime LastCompletedScheduledTimeUtc { get; set; }
    }
}
