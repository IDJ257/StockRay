using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Models;
using StockRay.Other;
using StockRay.Shared;
namespace StockRay.Services.AddSymbol
{



    public class AddSymbolService
    {
        private readonly ApplicationDbContext _context;

        public AddSymbolService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<ServiceResult<List<UserSymbolsOutboundDto>>> AddSymbolAsync(UserSymbolInboundDto inboundDto, int userId)
        {
            var user = await _context.Users
                .Include(u => u.Symbols)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new ArgumentNullException();


            //because of M:M relationship between user and symbol entities 
            //this is needed in order to not get the identity exception since 
            //in order the user to add a symbol it must be of Symbol();
            var symbols = await _context.Symbols.Where(s => inboundDto.SymbolIds.Contains(s.Id)).ToListAsync();


            //Eventualen hang
            List<UserSymbolsOutboundDto> addedSymbols = new List<UserSymbolsOutboundDto>();
         


            if (inboundDto.SymbolIds.Count == 1)
            {


                if (!user.TryAddSymbolToWatch(symbols[0]))
                {
                    return new ServiceResult<List<UserSymbolsOutboundDto>>(false);
                }


                await _context.SaveChangesAsync();

                return new ServiceResult<List<UserSymbolsOutboundDto>>(true, symbols
                        .Select(s => new UserSymbolsOutboundDto(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice)).ToList());

            }
            else
            {
                for (int i = 0; i < inboundDto.SymbolIds.Count; i++)
                {

                    if (!user.TryAddSymbolToWatch(symbols[i]))
                    {
                        continue;
                    }

                    addedSymbols.Add(new UserSymbolsOutboundDto(
                        symbols[i].Id,
                        symbols[i].Name,
                        symbols[i].Open,
                        symbols[i].High,
                        symbols[i].Low,
                        symbols[i].CurrentPrice
                        ));


                }


                await _context.SaveChangesAsync();

                return new ServiceResult<List<UserSymbolsOutboundDto>>(true, addedSymbols);
                        
            }










        }

    }


}












