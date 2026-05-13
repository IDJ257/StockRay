using Microsoft.EntityFrameworkCore;
using Quartz;
using StockRay.Database;
using StockRay.Models;
using System.Collections.Immutable;

namespace StockRay.BackGroundJobs.SetSymbolStateJob
{


    public interface ISetSymbolState
    {
        Task UpdateSymbolStateAsync();
    }

    public class SetSymbolState : ISetSymbolState
    {
        private readonly ApplicationDbContext _context;

        private readonly IFastAccess _fastAccess;

        public SetSymbolState(
            ApplicationDbContext context, IFastAccess fastAccess)
        {
            _context = context;
            _fastAccess = fastAccess;
        }

        public async Task UpdateSymbolStateAsync()
        {
            var symbols = await _context.Symbols.ToListAsync();


            if (symbols == null || symbols.Count == 0)
            {
                throw new ArgumentNullException(nameof(symbols));
            }

            //Moje task


            UpdatePrices(symbols);





            await _context.SaveChangesAsync();

            _fastAccess.Swap(SetUpImmutableList(symbols));


        }


        private ImmutableList<SymbolDto> SetUpImmutableList(List<Symbol> symbols)
        {

            return ImmutableList.CreateRange(symbols.Select(
                s => new SymbolDto(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice, s.IsTopNine)
                ));


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

                        if(newDecreasedPrice < 0)
                        {

                        }

                        if(randomValue < 0)
                        {

                        }

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
