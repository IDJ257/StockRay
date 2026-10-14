namespace StockRay.BackGroundJobs
{
    // All symbol jobs share this lock within this server process.
    public class BackgroundJobLock : IDisposable
    {
        public SemaphoreSlim Gate { get; }

        public BackgroundJobLock()
        {
            Gate = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            Gate.Dispose();
        }
    }
}
