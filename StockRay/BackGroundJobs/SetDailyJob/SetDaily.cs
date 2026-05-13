using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Models;
using System.Collections.Immutable;

namespace StockRay.BackGroundJobs.SetDailyJob
{
    //SCOPED
    public class SetDaily : ISetDaily
    {
        private readonly ApplicationDbContext _context;

        private readonly IFastAccess _fastAccess;

        public SetDaily(
            ApplicationDbContext context, IFastAccess fastAccess)
        {
            _context = context;
            _fastAccess = fastAccess;
        }
        public async Task SetDailyAsync()
        {
            List<Symbol> symbols = await _context.Symbols.ToListAsync();

            if (symbols == null || symbols.Count == 0)
            {

                //DA SE VIDI KAK DA SE HANDLENE PO-DORBE
                throw new ArgumentNullException(nameof(symbols));
            }


            //freevame threadove ot poola. Ne znam dali ima smisul shte vidim dali nqma da si ostane blocking code na calling threada 
            
        
                Random rnd = new Random();
                //ne sum siguren kakuv obekt shte se vrushta

                foreach (Symbol symbol in symbols)
                {
                    float initPrice = MathF.Round(rnd.NextSingle() * 100, 2);

                    if (initPrice == 0) initPrice += 1;

                    symbol.SetOpen(initPrice).SetHigh(initPrice).SetLow(initPrice).SetCurrentPrice(initPrice);
                }


                //RESET NA CLOSE AS WELL


           


            //moje da se hvurli exception sushto i toi moje da se hvane poneje e implicit tranzakciq da rollbacknem.
            await _context.SaveChangesAsync();


            _fastAccess.Swap(SetUpImmutableList(symbols));

        }


        private ImmutableList<SymbolDto> SetUpImmutableList(List<Symbol> symbols)
        {

            return ImmutableList.CreateRange(symbols.Select(
                s => new SymbolDto(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice, s.IsTopNine)
                ));


        }


    }


    public interface ISetDaily
    {
        Task SetDailyAsync();
    }
}
