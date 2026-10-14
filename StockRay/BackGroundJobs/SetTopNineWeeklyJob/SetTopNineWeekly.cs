using Microsoft.EntityFrameworkCore;
using StockRay.BackGroundJobs.ControlledExecutionOfJobs;
using StockRay.Database;
using StockRay.Models;
using System.Collections.Immutable;

namespace StockRay.BackGroundJobs.SetTopNineWeeklyJob
{
    //Job for setting the 9 symbols that are shown on /PublicDash
    public class SetTopNineWeekly : ISetTopNineWeekly
    {
        private readonly ApplicationDbContext _context;

        private readonly IFastAccess _fastAccess;

        private readonly JobCompletionRecords _completionRecords;

        private const int LEN = 9;

        public SetTopNineWeekly(
            ApplicationDbContext context, IFastAccess fastAccess,
            JobCompletionRecords completionRecords)
        {
            _context = context;
            _fastAccess = fastAccess;
            _completionRecords = completionRecords;
        }

        public async Task SetTopNineAsync(DateTime scheduledTimeUtc)
        {
            //0 - 99 indexa counta e 100
            var symbols = await _context.Symbols.ToListAsync();

            if (symbols == null || symbols.Count == 0)
            {

                throw new ArgumentNullException(nameof(symbols));
            }


            symbols.ForEach(s => s.DeSetTopNine());

            //Po-lesno e da se raboti vurhu kolekciqta i da callne DeSet na vseki element otkolkoto da mi vrushta spisuk s tezi koito sa TRUE
            //i da im natiska DESET i taka sh imam samo 

            symbols.OrderByDescending(s => s.High).Take(LEN).ToList().ForEach(s => s.SetTopNine());

            await _completionRecords.RecordAsync(JobSchedule.WeeklyTopNineName, scheduledTimeUtc);

            // EF saves the selection and completion row in one transaction.
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


    public interface ISetTopNineWeekly
    {
        Task SetTopNineAsync(DateTime scheduledTimeUtc);
    }
}
