using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Other;
using StockRay.Shared;
namespace StockRay.Services.RemoveSymbol
{




    public class RemoveSymbolService
    {

        private readonly ApplicationDbContext _context;

        public RemoveSymbolService(ApplicationDbContext context)
        {
            _context = context;
        }

        //Removing symbols from user entity
        public async Task<ServiceResult> RemoveSymbolAsync(int userId, UserSymbolInboundDto inboundDto)
        {
            var user = await _context.Users
                .Include(u => u.Symbols)
                .FirstOrDefaultAsync(u => u.Id == userId) 
                ?? throw new ArgumentNullException();

            //because of M:M relationship between user and symbol entities 
            //this is needed in order to not get the identity exception since 
            //in order the user to add a symbol it must be of Symbol();
            var symbolsToRemove = await _context.Symbols.Where(s => inboundDto.SymbolIds.Contains(s.Id)).ToListAsync();


            if (inboundDto.SymbolIds.Count == 1)
            {


                if (!user.RemoveSymbolFromWatch(symbolsToRemove[0]))
                {
                    return new ServiceResult(false);
                }


                await _context.SaveChangesAsync();

                return new ServiceResult(true);
            }
            else
            {
                for (int i = 0; i < inboundDto.SymbolIds.Count; i++)
                {

                    if (!user.RemoveSymbolFromWatch(symbolsToRemove[i]))
                    {
                        continue;
                        //return new ServiceResult(false);
                    }

                }

                await _context.SaveChangesAsync();

                return new ServiceResult(true);
            }




        }



    }


}
