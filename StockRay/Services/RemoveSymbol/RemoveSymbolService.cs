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


            public async Task<ServiceResult> RemoveSymbolAsync(int userId, UserSymbolInboundDto inboundDto)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId) ?? throw new ArgumentNullException();

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
                            return new ServiceResult(false);
                        }

                    }

                    await _context.SaveChangesAsync();

                    return new ServiceResult(true);
                }




            }



        }


}
