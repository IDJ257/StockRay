using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace StockRay.BackGroundJobs.SetTopNineWeeklyJob
{
    [DisallowConcurrentExecution]

    public class SetTopNineWeeklyJob : IJob
    {
        private readonly IScheduledJobs _scheduledJobs;
        private readonly ILogger<SetTopNineWeeklyJob> _logger;

        public SetTopNineWeeklyJob(IScheduledJobs scheduledJobs, ILogger<SetTopNineWeeklyJob> logger)
        {
            _scheduledJobs = scheduledJobs;
            _logger = logger;

        }

        public async Task Execute(IJobExecutionContext context)
        {

            try
            {
                await _scheduledJobs.RunDueAsync(context.CancellationToken);

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Weekly schedule check failed. Completion remains due for recovery.");
                throw new JobExecutionException(exception, false);
            }
          
        }
    }
}
