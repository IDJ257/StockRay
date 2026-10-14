using Quartz;

namespace StockRay.BackGroundJobs.SetSymbolStateJob
{

    [DisallowConcurrentExecution]
    public class SetSymbolStateJob : IJob
    {

        private readonly IScheduledJobs _scheduledJobs;
        private readonly ILogger<SetSymbolStateJob> _logger;


        public SetSymbolStateJob(IScheduledJobs scheduledJobs, ILogger<SetSymbolStateJob> logger)
        {
            _scheduledJobs = scheduledJobs;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            try
            {
                await _scheduledJobs.UpdatePricesAsync(context.CancellationToken);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Price update or schedule recovery failed. Another attempt is scheduled in 30 seconds.");
                throw new JobExecutionException(ex, false);
            }

        }



    }
}
