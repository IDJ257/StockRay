using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using System.Collections.Immutable;

namespace StockRay.BackGroundJobs.OnStartUpJob
{

    public interface IOnStartUp
    {
        Task BuildInitialAsync();
    }

    public class OnStartUp : IOnStartUp
    {

        private readonly IFastAccess _fastAccess;

        private readonly ApplicationDbContext _context;

        public OnStartUp(IFastAccess fastAccess, ApplicationDbContext context)
        {
            _fastAccess = fastAccess;
            _context = context;
        }

        public async Task BuildInitialAsync()
        {

            var symbols = await _context.Symbols.ToListAsync() ?? throw new ArgumentNullException();

            var immList = ImmutableList.CreateRange(symbols.Select(
                s => new SymbolDto(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice, s.IsTopNine)
                ));

            _fastAccess.Swap(immList);

          



        }


    }
}
