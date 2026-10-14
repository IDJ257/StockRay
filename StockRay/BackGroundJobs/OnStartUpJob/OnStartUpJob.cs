using Quartz;
using StockRay.BackGroundJobs.ControlledExecutionOfJobs;

namespace StockRay.BackGroundJobs.OnStartUpJob
{
    [DisallowConcurrentExecution]
    public class OnStartUpJob : IJob
    {
        private readonly IScheduledJobs _scheduledJobs;
        private readonly ILogger<OnStartUpJob> _logger;

        public OnStartUpJob(IScheduledJobs scheduledJobs, ILogger<OnStartUpJob> logger)
        {
            _scheduledJobs = scheduledJobs;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _scheduledJobs.InitializeAsync(context.CancellationToken);
            }
            catch (Exception exception)
            {

                _logger.LogError(exception, "Startup recovery failed. Uncompleted jobs will be checked again by the price job.");
                throw new JobExecutionException(exception, false);
            }
        }
    }
}
