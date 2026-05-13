using Quartz;

namespace StockRay.BackGroundJobs.SetSymbolStateJob
{

    [DisallowConcurrentExecution]
    public class SetSymbolStateJob : IJob
    {

        private readonly ISetSymbolState _setSymbolState;


        public SetSymbolStateJob(ISetSymbolState setSymbolState)
        {
            _setSymbolState = setSymbolState;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            try
            {
                await _setSymbolState.UpdateSymbolStateAsync();
            }
            catch (Exception ex)
            {

                throw new JobExecutionException(ex,false);
            }

        }



    }
}
