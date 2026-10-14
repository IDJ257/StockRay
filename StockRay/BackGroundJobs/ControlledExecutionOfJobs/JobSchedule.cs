namespace StockRay.BackGroundJobs.ControlledExecutionOfJobs
{
    public class JobSchedule
    {
        public const string DailyResetName = "DailyReset";
        public const string WeeklyTopNineName = "WeeklyTop9";

        public TimeZoneInfo TimeZone { get; }

        public JobSchedule(TimeZoneInfo timeZone)
        {
            TimeZone = timeZone;
        }

        public DateTime GetLatestDailyTimeUtc(DateTimeOffset now)
        {
            DateTime localTime = TimeZoneInfo.ConvertTime(now, TimeZone).DateTime;
            DateTime scheduledTime = localTime.Date.AddHours(6);

            if (localTime < scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(-1);
            }

            return TimeZoneInfo.ConvertTimeToUtc(scheduledTime, TimeZone);
        }


        //Vzimame VMOMENTNOTO UTC vreme i vrushtame v kolko chasa DNES e trqbvalo da mine weekly job.
        public DateTime GetLatestWeeklyTimeUtc(DateTimeOffset now)
        {
            DateTime localTime = TimeZoneInfo.ConvertTime(now, TimeZone).DateTime;
            int daysSinceSunday = (int)localTime.DayOfWeek;
            DateTime scheduledTime = localTime.Date.AddDays(-daysSinceSunday).AddHours(6);

            if (localTime < scheduledTime)
            {
                scheduledTime = scheduledTime.AddDays(-7);
            }

            return TimeZoneInfo.ConvertTimeToUtc(scheduledTime, TimeZone);
        }
    }
}
