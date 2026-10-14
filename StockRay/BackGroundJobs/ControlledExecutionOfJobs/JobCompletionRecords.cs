using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Models;

namespace StockRay.BackGroundJobs.ControlledExecutionOfJobs
{
    public class JobCompletionRecords
    {
        private readonly ApplicationDbContext _context;

        public JobCompletionRecords(ApplicationDbContext context)
        {
            _context = context;
        }

        //Proverqvame dali nqkoi JOB e izbqgal i trqbva da se pravi. T.e scheduledTimeUTC e dadenoto nashe Vreme za daden JOB.
        //T.e ako weekly smenqme top9 v Nedelq 6:00 sme zapazili che na 11/10/2026 v 6:00 imame COMPLETED JOB RECORD
        //ako servera crashne v nedelq v 5:59 i otvori v 6:01 to togava tova runva pak i vijdame che posledniq JOB e na 11/10/2026 v 6;00,
        //A SEGA SME 6:01 17/10/2026 naprimer


        //Da moje da  e leko repetetive ama e readable ne kato predishnata pomiq
        public async Task<bool> IsWeeklyDueAsync(DateTime scheduledTimeUtc)
        {
            var completion = await _context.JobCompletions
                .SingleOrDefaultAsync(record => record.JobName == JobSchedule.WeeklyTopNineName);

            if (completion == null)
            {
                return true;
            }

            return completion.LastCompletedScheduledTimeUtc < scheduledTimeUtc;
        }

        public async Task<bool> IsDailyDueAsync(DateTime scheduledTimeUtc)
        {
            var completion = await _context.JobCompletions
                .SingleOrDefaultAsync(record => record.JobName == JobSchedule.DailyResetName);

            if (completion == null)
            {
                return true;
            }

            return completion.LastCompletedScheduledTimeUtc < scheduledTimeUtc;
        }



    
        //Tuk proverqvame dali imame veche takuv job ako ne suzdavame i dobavqme che daden JOB e bil IZPULNEN VUV DADENOTO OT NAS VREME
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
