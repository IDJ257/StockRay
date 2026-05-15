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


        //Eventualno da se vrushta List<OutboundDto> za da moje vednaga da se pokajat promenite
        public async Task<ServiceResult<List<UserSymbolsOutboundDto>>> AddSymbolAsync(UserSymbolInboundDto inboundDto, int userId)
        {
            var user = await _context.Users
                .Include(u => u.Symbols)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new ArgumentNullException();


            //Za sega sa dve querita NO, moje bi trqvba da minem na ID-centric M:M tablica kudeto
            //prosto shte dobavqme simvoli kum useri chrez tehnite ID-ta.
            var symbols = await _context.Symbols.Where(s => inboundDto.SymbolIds.Contains(s.Id)).ToListAsync();

         



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
                        return new ServiceResult<List<UserSymbolsOutboundDto>>(false);
                    }



                }


                await _context.SaveChangesAsync();

                return new ServiceResult<List<UserSymbolsOutboundDto>>(true, symbols
                        .Select(s => new UserSymbolsOutboundDto(s.Id, s.Name, s.Open, s.High, s.Low, s.CurrentPrice)).ToList());
            }










        }

    }


}












