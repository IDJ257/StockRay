using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace StockRay.BackGroundJobs.SetTopNineWeeklyJob
{
    [DisallowConcurrentExecution]

    public class SetTopNineWeeklyJob : IJob
    {
        private readonly ISetTopNineWeekly _setTopNineWeekly;

        public SetTopNineWeeklyJob(ISetTopNineWeekly setTopNineWeekly)
        {
            _setTopNineWeekly = setTopNineWeekly;

        }

        public async Task Execute(IJobExecutionContext context)
        {

            try
            {
                await _setTopNineWeekly.SetTopNineAsync();

            }
            catch (Exception)
            {
                throw new JobExecutionException(false);
            }
          
        }
    }
}
