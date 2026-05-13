using Microsoft.Extensions.Caching.Memory;
using Quartz;
using System.Runtime.Serialization.Formatters;

namespace StockRay.BackGroundJobs.SetDailyJob
{
    [DisallowConcurrentExecution]

    public class SetDailyJob : IJob
    {
        //NA VSEKI 24H

        private readonly ISetDaily _setDaily;

        public SetDailyJob(ISetDaily setDaily)
        {
            _setDaily = setDaily;

        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _setDaily.SetDailyAsync();


            }
            catch (Exception)
            {
                //moje da se logva exceptiona 
                // za sega ne ni dreme tui kato moje da failne samo DB-to nishto drugo 
                throw new JobExecutionException(false);
            }
        

        }
    }
}
