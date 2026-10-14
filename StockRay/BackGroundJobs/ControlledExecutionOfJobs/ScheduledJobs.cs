using StockRay.BackGroundJobs.OnStartUpJob;
using StockRay.BackGroundJobs.SetDailyJob;
using StockRay.BackGroundJobs.SetSymbolStateJob;
using StockRay.BackGroundJobs.SetTopNineWeeklyJob;

namespace StockRay.BackGroundJobs.ControlledExecutionOfJobs
{
    public interface IScheduledJobs
    {
        Task RunScheduledJobsAsync(CancellationToken cancellationToken);

        Task InitializeAsync(CancellationToken cancellationToken);

        Task UpdatePricesAsync(CancellationToken cancellationToken);
    }

    public class ScheduledJobs : IScheduledJobs
    {
        private readonly JobSchedule _schedule;
        private readonly JobCompletionRecords _completionRecords;
        private readonly BackgroundJobLock _jobLock;
        private readonly JobExecutionState _executionState;
        private readonly ISetDaily _setDaily;
        private readonly ISetTopNineWeekly _setTopNine;
        private readonly ISetSymbolState _setSymbolState;
        private readonly IOnStartUp _onStartUp;
        private readonly ILogger<ScheduledJobs> _logger;


        //Baq injection ima trqq se refactorne 
        public ScheduledJobs(
            JobSchedule schedule,
            JobCompletionRecords completionRecords,
            BackgroundJobLock jobLock,
            JobExecutionState executionState,
            ISetDaily setDaily,
            ISetTopNineWeekly setTopNine,
            ISetSymbolState setSymbolState,
            IOnStartUp onStartUp,
            ILogger<ScheduledJobs> logger)
        {
            _schedule = schedule;
            _completionRecords = completionRecords;
            _jobLock = jobLock;
            _executionState = executionState;
            _setDaily = setDaily;
            _setTopNine = setTopNine;
            _setSymbolState = setSymbolState;
            _onStartUp = onStartUp;
            _logger = logger;
        }

        public async Task RunScheduledJobsAsync(CancellationToken cancellationToken)
        {
            await _jobLock.Gate.WaitAsync(cancellationToken);

            try
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // Top9 must use the existing highs before the daily reset clears them.
                await RunWeeklyIfNeededAsync(now);
                await RunDailyIfNeededAsync(now);
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
                DateTimeOffset now = DateTimeOffset.UtcNow;

                await RunWeeklyIfNeededAsync(now);
                await RunDailyIfNeededAsync(now);
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
                DateTimeOffset now = DateTimeOffset.UtcNow;


                //tezi ne pravqt DB tripove veche vseki 30-sekundi a samo ako imame DUE shoto imame flagovete v JobExecutionState.cs
                await RunWeeklyIfNeededAsync(now);
                await RunDailyIfNeededAsync(now);
                await _setSymbolState.UpdateSymbolStateAsync();
            }
            finally
            {
                _jobLock.Gate.Release();
            }
        }

        private async Task RunWeeklyIfNeededAsync(DateTimeOffset now)
        {
            DateTime weeklyTimeUtc = _schedule.GetLatestWeeklyTimeUtc(now);
            DateTime? checkedWeeklyTimeUtc = _executionState.LastCheckedWeeklyTimeUtc;

            if (checkedWeeklyTimeUtc.HasValue && checkedWeeklyTimeUtc.Value >= weeklyTimeUtc)
            {
                return;
            }

            bool weeklyIsDue = await _completionRecords.IsWeeklyDueAsync(weeklyTimeUtc);

            if (weeklyIsDue)
            {
                _logger.LogInformation("Running Top9 for {ScheduledTimeUtc}.", weeklyTimeUtc);
                await _setTopNine.SetTopNineAsync(weeklyTimeUtc);
                _logger.LogInformation("Completed Top9 for {ScheduledTimeUtc}.", weeklyTimeUtc);
            }

            _executionState.LastCheckedWeeklyTimeUtc = weeklyTimeUtc;
        }

        private async Task RunDailyIfNeededAsync(DateTimeOffset now)
        {
            DateTime dailyTimeUtc = _schedule.GetLatestDailyTimeUtc(now);
            DateTime? checkedDailyTimeUtc = _executionState.LastCheckedDailyTimeUtc;



            //Izbqgvane na db tripove vseki 30 sekundi naprimer zaradi joba koito smenq cenite vseki 30 sekundi
            //proverqvame dali ima stoinost i ako ima checkedDailyTimeUtc vinagi shte bude 6:00 AM na posledniq den
            //Naprimer ako sme srqda 5:50 sutrinta checkedDailyTimeUtc == Vtornik 6:00 AM,
            //A DailyTimeUtc shte bude Srqda 6:00 no poneje 5:50 e < pak shte izleznem samo kogato e 6:00 sreshtu 6:00 s razlichnite dni shte runnem neshtata
            if (checkedDailyTimeUtc.HasValue && checkedDailyTimeUtc.Value >= dailyTimeUtc)
            {
                return;
            }

            bool dailyIsDue = await _completionRecords.IsDailyDueAsync(dailyTimeUtc);

            if (dailyIsDue)
            {
                _logger.LogInformation("Running daily reset for {ScheduledTimeUtc}.", dailyTimeUtc);
                await _setDaily.SetDailyAsync(dailyTimeUtc);  //Normal job run
                _logger.LogInformation("Completed daily reset for {ScheduledTimeUtc}.", dailyTimeUtc);
            }

            _executionState.LastCheckedDailyTimeUtc = dailyTimeUtc;
        }
    }
}
