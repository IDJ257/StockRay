using Microsoft.Extensions.Caching.Memory;
using Quartz;
using StockRay.BackGroundJobs.ControlledExecutionOfJobs;
using System.Runtime.Serialization.Formatters;

namespace StockRay.BackGroundJobs.SetDailyJob
{
    [DisallowConcurrentExecution]

    public class SetDailyJob : IJob
    {
        //NA VSEKI 24H

        private readonly IScheduledJobs _scheduledJobs;
        private readonly ILogger<SetDailyJob> _logger;

        public SetDailyJob(IScheduledJobs scheduledJobs, ILogger<SetDailyJob> logger)
        {
            _scheduledJobs = scheduledJobs;
            _logger = logger;

        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _scheduledJobs.RunScheduledJobsAsync(context.CancellationToken);


            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Daily schedule check failed. Completion remains due for recovery.");
                throw new JobExecutionException(exception, false);
            }
        

        }
    }
}
