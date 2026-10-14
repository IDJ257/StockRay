# Background job recovery

The `JobCompletions` table records the last successful scheduled occurrence
for each job. It contains at most two rows:

| JobName | LastCompletedScheduledTimeUtc |
| --- | --- |
| DailyReset | The latest daily 06:00 occurrence completed successfully, stored in UTC |
| WeeklyTop9 | The latest Sunday 06:00 occurrence completed successfully, stored in UTC |

The timestamp represents the scheduled occurrence, not the actual finishing
time. A reset performed Tuesday at 06:01 records Tuesday's scheduled 06:00.

## Database update required

Apply the `TrackJobCompletions` migration before running this version of the
server. In Visual Studio's Package Manager Console, with StockRay selected:

```powershell
Update-Database
```

If the `dotnet-ef` tool is installed, the equivalent repository-root command is:

```powershell
dotnet ef database update --project StockRay/StockRay.csproj
```

The migration adds the table without modifying existing symbols. No migration
is applied automatically by the application. Without the table, the recovery
checks fail and the price updater cannot proceed.

There are no initial completion rows. On the first startup after applying the
migration, Top9 selection and the daily reset each run once for their latest
due occurrence. This changes the existing selection and prices.

## How recovery works

`JobSchedule` calculates the most recent daily and weekly scheduled times.
`JobCompletionRecords` compares them against the two database rows.
`ScheduledJobs` runs overdue work in this order:

1. Select Top9 from the existing highs.
2. Reset daily prices.
3. Allow the normal price update to proceed.

Both Quartz schedule triggers use this same check, so their firing order does
not affect the Top9-before-reset ordering. Startup also uses this check, and
the existing 30-second price trigger checks again before updating prices.

Each job prepares its symbol changes and completion record in the same scoped
DbContext, then saves them in one `SaveChangesAsync` transaction. A failed save
does not advance completion. The cache is replaced only after a successful save.

If the server crashes after a successful commit, startup loads the committed
symbols and skips that completed occurrence. If a reset fails, it remains due
and the next price tick attempts recovery again. There is no immediate retry
loop or separate retry timer. Original exceptions are logged and retained in
the Quartz exception.

When several occurrences were missed, recovery runs only the latest occurrence
for each job. It does not replay every missed daily randomization or weekly
selection. A delayed Top9 selection uses the highs currently stored in the
database; historical highs from a missed period cannot be reconstructed.

## Timezone and ordering

Quartz triggers and recovery calculations use the same timezone. By default,
this is the server's local timezone, preserving the existing schedule. An
explicit timezone can be configured in `appsettings.json`:

```json
"BackgroundJobs": {
  "TimeZone": "Europe/Sofia"
}
```

Daily resets remain at 06:00 every day. Top9 remains at Sunday 06:00 and runs
before that morning's reset. This uses highs accumulated since Saturday's
06:00 reset; it does not represent the completed Sunday trading period.

`BackgroundJobLock` prevents startup, schedule checks, and price updates from
overlapping within one server process. This implementation assumes one server
instance; the lock does not coordinate multiple servers sharing a database.

## Files to read

- `JobSchedule.cs`: finding the latest due daily and weekly occurrences.
- `JobCompletionRecords.cs`: reading and preparing the database records.
- `ScheduledJobs.cs`: recovery, ordering, and the shared lock.
- `SetDailyJob/SetDaily.cs`: saving the reset together with its completion.
- `SetTopNineWeeklyJob/SetTopNineWeekly.cs`: saving the selection together with its completion.

The implementation was reviewed statically after the request to leave running
and verification to the developer. No test project is included.
