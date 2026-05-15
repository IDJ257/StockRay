using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using StockRay.Database;
using StockRay.Models;
using StockRay.Other;
using StockRay.Services.GetSymbol;
using StockRay.Shared;
using StockRay.SignalHub;
using System.Collections.Immutable;
using System.Net.NetworkInformation;

namespace StockRay.BackGroundJobs.SetSymbolStateJob
{

    //AKO HANGNE MOJE DA E OTTUK
    //HANG!


    public interface ISetSymbolState
    {
        Task UpdateSymbolStateAsync();
    }


    //REFACTOR!!
    public class SetSymbolState : ISetSymbolState
    {
        private readonly ApplicationDbContext _context;

        private readonly IFastAccess _fastAccess;

        private readonly IHubContext<SymbolNotifHub, ISymbolNotifClient> _hubContext;

        private readonly IActiveGroup _activeGroup;



        public SetSymbolState(
            ApplicationDbContext context,
            IFastAccess fastAccess,
            IHubContext<SymbolNotifHub, ISymbolNotifClient> hubContext,
            IActiveGroup activeGroup
           )
        {
            _context = context;
            _fastAccess = fastAccess;
            _hubContext = hubContext;
            _activeGroup = activeGroup;

        }

        public async Task UpdateSymbolStateAsync()
        {
            var symbols = await _context.Symbols
                .AsSplitQuery()
                .ToListAsync();




            if (symbols == null || symbols.Count == 0)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            //Moje task



            UpdatePrices(symbols);

            await _context.SaveChangesAsync();

            _fastAccess.Swap(SetUpImmutableList(symbols));

            await SendToWebSocket();







        }


        private ImmutableList<SymbolDto> SetUpImmutableList(List<Symbol> symbols)
        {

            return ImmutableList.CreateRange(symbols.Select(
                s => new SymbolDto(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice, s.IsTopNine)
                ));


        }

        private async Task SendToWebSocket()
        {

            //HANG?
            //Moje da ima hang zaradi sled awaita gore pri saveChanges se smenq threada
            //Ne sum siguren dali ima smisul ot tova sega da sa fire-forget ama 
            //nqma razlika v stiganeto po websocketa taka che moje 
            //List<Task> tasks = new List<Task>();

            var fastList = _fastAccess.GetSymbols();

            await _hubContext.Clients.Group("Public").ReceivePublicUpdate(
                fastList.Where(s => s.IsTopNine == true)
                .Select(s => new OutboundStockPrice(s.Id, s.Open, s.High, s.Low, s.CurrentPrice)).ToList());



            foreach (var item in _activeGroup.ConnectionGroups)
            {
                //fast list da mi dade za AAPL, MSFT, TSM

              var outBoundListForClient = fastList
                    .Where(s => item.Value.Contains(s.Name)) //NE BI TRQBVALO DA IMA READ HANG NA item.VALUE. VALUETO e OT CONCURRENTBAG TIP I READA BI TRQBVALO DA SE LOCKNE
                    .Select(es => new OutboundStockPrice(es.Id, es.Open, es.High, es.Low, es.CurrentPrice))
                    .ToList();


                await _hubContext.Clients.Client(item.Key).ReceiveGroupUpdate(outBoundListForClient);
            }

            


        }


        private void UpdatePrices(List<Symbol> symbols)
        {
            //ako e bavno neshto ili freezva moje da se sloji v TASK.RUN za sega ne
            Random rnd = new Random();
            foreach (Symbol symbol in symbols)
            {

                float randomValue = 0f;
                int randomState = rnd.Next(0, 2);

                //case 0: namalqme current cenata s procentna stoinost  case 1: uvelichavame current cenata s nqkakva stoinost
                switch (randomState)
                {

                    case 0:

                        randomValue = rnd.NextSingle() * symbol.CurrentPrice;

                        var newDecreasedPrice = symbol.CurrentPrice - randomValue;


                        if (newDecreasedPrice < symbol.Low)
                        {
                            symbol.SetCurrentPrice(newDecreasedPrice).SetLow(newDecreasedPrice);
                        }
                        else
                        {
                            symbol.SetCurrentPrice(newDecreasedPrice);
                        }

                        break;


                    case 1:

                        randomValue = MathF.Round(rnd.NextSingle() * 100f, 2);
                        var newIncreasedPrice = symbol.CurrentPrice + randomValue;

                        if (newIncreasedPrice > symbol.High)
                        {
                            symbol.SetCurrentPrice(newIncreasedPrice).SetHigh(newIncreasedPrice);
                        }
                        else
                        {
                            symbol.SetCurrentPrice(newIncreasedPrice);
                        }
                        break;
                    default:
                        //dano ne stignem che 
                        break;
                }
            }
        }
    }

}
