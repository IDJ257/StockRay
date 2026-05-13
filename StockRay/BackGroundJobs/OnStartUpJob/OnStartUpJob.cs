using Quartz;

namespace StockRay.BackGroundJobs.OnStartUpJob
{
    [DisallowConcurrentExecution]
    public class OnStartUpJob : IJob
    {
        private readonly IOnStartUp _onStartUp;

        public OnStartUpJob(IOnStartUp onStartUp)
        {
            _onStartUp = onStartUp;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _onStartUp.BuildInitialAsync();
            }
            catch (Exception)
            {

                throw new JobExecutionException();
            }
        }
    }
}
