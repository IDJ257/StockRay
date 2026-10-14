namespace StockRay.BackGroundJobs.ControlledExecutionOfJobs
{

    //Dva flaga za proverka dali VECHE ne sme izpulnili Due checkovete
    public class JobExecutionState
    {
        public DateTime? LastCheckedWeeklyTimeUtc { get; set; }

        public DateTime? LastCheckedDailyTimeUtc { get; set; }
    }
}
