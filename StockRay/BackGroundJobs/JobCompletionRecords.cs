using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Models;

namespace StockRay.BackGroundJobs
{
    public class JobCompletionRecords
    {
        private readonly ApplicationDbContext _context;

        public JobCompletionRecords(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsDueAsync(string jobName, DateTime scheduledTimeUtc)
        {
            var completion = await _context.JobCompletions
                .SingleOrDefaultAsync(record => record.JobName == jobName);

            if (completion == null)
            {
                return true;
            }

            return completion.LastCompletedScheduledTimeUtc < scheduledTimeUtc;
        }

        public async Task RecordAsync(string jobName, DateTime scheduledTimeUtc)
        {
            var completion = await _context.JobCompletions
                .SingleOrDefaultAsync(record => record.JobName == jobName);

            if (completion == null)
            {
                completion = new JobCompletion();
                completion.JobName = jobName;
                _context.JobCompletions.Add(completion);
            }

            completion.LastCompletedScheduledTimeUtc = scheduledTimeUtc;

            // Do not save here. The job saves this row together with its symbols.
        }
    }
}
