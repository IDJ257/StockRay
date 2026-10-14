using StockRay.BackGroundJobs.OnStartUpJob;
using StockRay.BackGroundJobs.SetDailyJob;
using StockRay.BackGroundJobs.SetSymbolStateJob;
using StockRay.BackGroundJobs.SetTopNineWeeklyJob;

namespace StockRay.BackGroundJobs
{
    public interface IScheduledJobs
    {
        Task RunDueAsync(CancellationToken cancellationToken);

        Task InitializeAsync(CancellationToken cancellationToken);

        Task UpdatePricesAsync(CancellationToken cancellationToken);
    }

    public class ScheduledJobs : IScheduledJobs
    {
        private readonly JobSchedule _schedule;
        private readonly JobCompletionRecords _completionRecords;
        private readonly BackgroundJobLock _jobLock;
        private readonly ISetDaily _setDaily;
        private readonly ISetTopNineWeekly _setTopNine;
        private readonly ISetSymbolState _setSymbolState;
        private readonly IOnStartUp _onStartUp;
        private readonly ILogger<ScheduledJobs> _logger;

        public ScheduledJobs(
            JobSchedule schedule,
            JobCompletionRecords completionRecords,
            BackgroundJobLock jobLock,
            ISetDaily setDaily,
            ISetTopNineWeekly setTopNine,
            ISetSymbolState setSymbolState,
            IOnStartUp onStartUp,
            ILogger<ScheduledJobs> logger)
        {
            _schedule = schedule;
            _completionRecords = completionRecords;
            _jobLock = jobLock;
            _setDaily = setDaily;
            _setTopNine = setTopNine;
            _setSymbolState = setSymbolState;
            _onStartUp = onStartUp;
            _logger = logger;
        }

        public async Task RunDueAsync(CancellationToken cancellationToken)
        {
            await _jobLock.Gate.WaitAsync(cancellationToken);

            try
            {
                await RunDueInsideLockAsync();
            }
            finally
            {
                _jobLock.Gate.Release();
            }
        }

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            await _jobLock.Gate.WaitAsync(cancellationToken);

            try
            {
                // Load existing data even if a later recovery attempt fails.
                await _onStartUp.BuildInitialAsync();
                await RunDueInsideLockAsync();
            }
            finally
            {
                _jobLock.Gate.Release();
            }
        }

        public async Task UpdatePricesAsync(CancellationToken cancellationToken)
        {
            await _jobLock.Gate.WaitAsync(cancellationToken);

            try
            {
                // Also recover a failed reset before changing prices again.
                // The existing 30-second trigger provides another attempt.
                await RunDueInsideLockAsync();
                await _setSymbolState.UpdateSymbolStateAsync();
            }
            finally
            {
                _jobLock.Gate.Release();
            }
        }

        private async Task RunDueInsideLockAsync()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTime weeklyTimeUtc = _schedule.GetLatestWeeklyTimeUtc(now);
            DateTime dailyTimeUtc = _schedule.GetLatestDailyTimeUtc(now);

            // Top9 must use the existing highs before the daily reset clears them.
            bool weeklyIsDue = await _completionRecords.IsDueAsync(
                JobSchedule.WeeklyTopNineName, weeklyTimeUtc);

            if (weeklyIsDue)
            {
                _logger.LogInformation("Running Top9 for {ScheduledTimeUtc}.", weeklyTimeUtc);
                await _setTopNine.SetTopNineAsync(weeklyTimeUtc);
                _logger.LogInformation("Completed Top9 for {ScheduledTimeUtc}.", weeklyTimeUtc);
            }

            bool dailyIsDue = await _completionRecords.IsDueAsync(
                JobSchedule.DailyResetName, dailyTimeUtc);

            if (dailyIsDue)
            {
                _logger.LogInformation("Running daily reset for {ScheduledTimeUtc}.", dailyTimeUtc);
                await _setDaily.SetDailyAsync(dailyTimeUtc);
                _logger.LogInformation("Completed daily reset for {ScheduledTimeUtc}.", dailyTimeUtc);
            }
        }
    }
}
