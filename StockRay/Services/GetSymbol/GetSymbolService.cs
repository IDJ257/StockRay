using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Other;
using StockRay.Shared;
namespace StockRay.Services.GetSymbol
{



    public class GetSymbolService
    {
        private readonly ApplicationDbContext _context;

        private readonly IFastAccess _fastAccess;

        public GetSymbolService(
            ApplicationDbContext context,
            IFastAccess fastAccess)
        {
            _context = context;
            _fastAccess = fastAccess;
        }


        public async Task<ServiceResult<List<UserSymbolsOutboundDto>>> GetSymbolsAsync(int userId)
        {
            // po gramoten error handling 
            //moje bi da se vrushta serviceResult
            var symbolIds = await _context.Users.Where(u => u.Id == userId)
                .SelectMany(s => s.Symbols)
                .Select(ls => ls.Id)
                .ToListAsync() ?? throw new ArgumentNullException();


            var outBoundList = _fastAccess.
                GetSymbols()
                .Where(s => symbolIds.Contains(s.Id))
                .Select(each => new UserSymbolsOutboundDto(each.Id, each.Name, each.Open, each.High, each.Low, each.CurrentPrice))
                .ToList();


            return new ServiceResult<List<UserSymbolsOutboundDto>>(true, outBoundList);








        }


    }

}



