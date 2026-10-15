using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Quartz;
using StockRay;
using StockRay.BackGroundJobs.ControlledExecutionOfJobs;
using StockRay.BackGroundJobs.OnStartUpJob;
using StockRay.BackGroundJobs.SetDailyJob;
using StockRay.BackGroundJobs.SetSymbolStateJob;
using StockRay.BackGroundJobs.SetTopNineWeeklyJob;
using StockRay.Database;
using StockRay.Models;
using StockRay.Other;
using StockRay.SignalHub;

int passed = 0;
async Task Check(string name, Func<Task> test)
{
    await test();
    Console.WriteLine($"PASS {name}");
    passed++;
}
void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
var schedule = new JobSchedule(TimeZoneInfo.FindSystemTimeZoneById("Europe/Sofia"));
await Check("Daily/weekly boundaries including both daylight-saving transitions", () =>
{
    foreach (var day in new[] { "2026-03-29", "2026-10-25", "2026-10-11" })
    {
        var local = DateTime.SpecifyKind(DateTime.Parse(day).AddHours(6), DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(local, schedule.TimeZone);
        Assert(schedule.GetLatestDailyTimeUtc(utc) == utc, "Daily boundary");
        Assert(schedule.GetLatestWeeklyTimeUtc(utc) == utc, "Weekly boundary");
        Assert(schedule.GetLatestDailyTimeUtc(utc.AddTicks(-1)) == TimeZoneInfo.ConvertTimeToUtc(local.AddDays(-1), schedule.TimeZone), "Before daily boundary");
        Assert(schedule.GetLatestWeeklyTimeUtc(utc.AddTicks(-1)) == TimeZoneInfo.ConvertTimeToUtc(local.AddDays(-7), schedule.TimeZone), "Before weekly boundary");
    }
    return Task.CompletedTask;
});

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
using var db = new ApplicationDbContext(options);
db.Database.EnsureCreated();
db.Symbols.RemoveRange(db.Symbols);
await db.SaveChangesAsync();
for (int i = 1; i <= 12; i++) db.Symbols.Add(new Symbol($"TEST{i}").SetOpen(i).SetHigh(i * 10).SetLow(i).SetCurrentPrice(i));
await db.SaveChangesAsync();
var cache = new FastAccess();
var records = new JobCompletionRecords(db);
var startup = new OnStartUp(cache, db);
var weekly = new SetTopNineWeekly(db, cache, records);
var daily = new SetDaily(db, cache, records);
var now = DateTimeOffset.UtcNow;
var weeklyTime = schedule.GetLatestWeeklyTimeUtc(now);
var dailyTime = schedule.GetLatestDailyTimeUtc(now);
await Check("Startup populates cache", async () =>
{
    await startup.BuildInitialAsync();
    Assert(cache.GetSymbols().Count == 12, "Cache count");
});
await Check("Weekly selects nine highest symbols and persists completion", async () =>
{
    Assert(await records.IsWeeklyDueAsync(weeklyTime), "Initially due");
    await weekly.SetTopNineAsync(weeklyTime);
    db.ChangeTracker.Clear();
    var selected = await db.Symbols.Where(s => s.IsTopNine).Select(s => s.Name).ToListAsync();
    Assert(selected.Order().SequenceEqual(Enumerable.Range(4, 9).Select(i => $"TEST{i}").Order()), "Wrong Top Nine");
    Assert(!await records.IsWeeklyDueAsync(weeklyTime), "Completion absent");
    Assert(await records.IsWeeklyDueAsync(weeklyTime.AddDays(7)), "Next week not due");
    Assert(cache.GetSymbols().Count(s => s.IsTopNine) == 9, "Cache flags");
});
await Check("Daily resets OHLC inputs, preserves selection, persists completion/cache", async () =>
{
    await daily.SetDailyAsync(dailyTime);
    db.ChangeTracker.Clear();
    var symbols = await db.Symbols.ToListAsync();
    Assert(symbols.All(s => s.Open > 0 && s.Open <= 100 && s.High == s.Open && s.Low == s.Open && s.CurrentPrice == s.Open), "Reset values");
    Assert(symbols.Count(s => s.IsTopNine) == 9, "Selection changed");
    Assert(!await records.IsDailyDueAsync(dailyTime), "Completion absent");
    Assert(cache.GetSymbols().All(c => symbols.Any(s => s.Id == c.Id && s.CurrentPrice == c.CurrentPrice)), "Stale cache");
});
int publicSends = 0, privateSends = 0;
bool failSend = false;
TaskCompletionSource? blockedSend = null;
TaskCompletionSource? sendEntered = null;
var client = Stub.Create<ISymbolNotifClient>((m, args) =>
{
    if (blockedSend != null)
    {
        sendEntered!.TrySetResult();
        return blockedSend.Task;
    }
    if (failSend) throw new InvalidOperationException("Simulated transport failure");
    if (m.Name == "ReceivePublicUpdate") publicSends++; else privateSends++;
    return Task.CompletedTask;
});
var clients = Stub.Create<IHubClients<ISymbolNotifClient>>((m, args) => client);
var hub = Stub.Create<IHubContext<SymbolNotifHub, ISymbolNotifClient>>((m, args) => clients);
var groups = new ActiveGroup();
groups.AddToGroup("test-client", new List<string> { "TEST1" });
var prices = new SetSymbolState(db, cache, hub, groups);
await Check("Price updates preserve bounds and send public/private notifications", async () =>
{
    for (int i = 0; i < 20; i++) await prices.UpdateSymbolStateAsync();
    db.ChangeTracker.Clear();
    Assert((await db.Symbols.ToListAsync()).All(s => s.Low <= s.CurrentPrice && s.CurrentPrice <= s.High && s.Low >= 0), "Price bounds");
    Assert(publicSends == 20 && privateSends == 20, "Missing notifications");
});
using var gate = new BackgroundJobLock();
var state = new JobExecutionState();
var runner = new ScheduledJobs(schedule, records, gate, state, daily, weekly, prices, startup, NullLogger<ScheduledJobs>.Instance);
await Check("Repeated/concurrent schedule checks skip completed resets", async () =>
{
    var before = cache.GetSymbols();
    await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => runner.RunScheduledJobsAsync(default)));
    Assert(ReferenceEquals(before, cache.GetSymbols()), "Reset unexpectedly ran");
    Assert(state.LastCheckedDailyTimeUtc == dailyTime && state.LastCheckedWeeklyTimeUtc == weeklyTime, "Execution state");
});
await Check("Failed broadcast releases shared gate and subsequent update recovers", async () =>
{
    failSend = true;
    try { await runner.UpdatePricesAsync(default); throw new Exception("Expected failure"); }
    catch (InvalidOperationException) { }
    Assert(gate.Gate.CurrentCount == 1, "Gate leaked");
    failSend = false;
    await runner.UpdatePricesAsync(default);
});
await Check("Cancelled lock wait does not release another caller's gate", async () =>
{
    await gate.Gate.WaitAsync();
    using var cts = new CancellationTokenSource();
    cts.Cancel();
    try { await runner.RunScheduledJobsAsync(cts.Token); throw new Exception("Expected cancellation"); }
    catch (OperationCanceledException) { }
    Assert(gate.Gate.CurrentCount == 0, "Incorrect release");
    gate.Gate.Release();
});
await Check("Reproduce: blocked notification holds all jobs despite cancellation", async () =>
{
    blockedSend = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    sendEntered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    using var cts = new CancellationTokenSource();
    var updating = runner.UpdatePricesAsync(cts.Token);
    await sendEntered.Task.WaitAsync(TimeSpan.FromSeconds(5));
    cts.Cancel();
    var scheduled = runner.RunScheduledJobsAsync(default);
    Assert(!updating.IsCompleted && !scheduled.IsCompleted && gate.Gate.CurrentCount == 0, "Expected shared-lock blockage");
    var release = blockedSend;
    blockedSend = null;
    release.SetResult();
    await Task.WhenAll(updating, scheduled).WaitAsync(TimeSpan.FromSeconds(5));
});
await Check("All four Quartz wrappers dispatch and wrap failures", async () =>
{
    var calls = new List<string>();
    bool fail = false;
    var jobs = Stub.Create<IScheduledJobs>((m, args) => { calls.Add(m.Name); return fail ? Task.FromException(new InvalidOperationException("test")) : Task.CompletedTask; });
    var context = Stub.Create<IJobExecutionContext>((m, args) => CancellationToken.None);
    IJob[] wrappers = { new OnStartUpJob(jobs, NullLogger<OnStartUpJob>.Instance), new SetDailyJob(jobs, NullLogger<SetDailyJob>.Instance), new SetTopNineWeeklyJob(jobs, NullLogger<SetTopNineWeeklyJob>.Instance), new SetSymbolStateJob(jobs, NullLogger<SetSymbolStateJob>.Instance) };
    foreach (var wrapper in wrappers) await wrapper.Execute(context);
    Assert(calls.SequenceEqual(new[] { "InitializeAsync", "RunScheduledJobsAsync", "RunScheduledJobsAsync", "UpdatePricesAsync" }), "Wrong delegation");
    fail = true;
    foreach (var wrapper in wrappers)
    {
        try { await wrapper.Execute(context); throw new Exception("Expected failure"); }
        catch (JobExecutionException) { }
    }
});
await Check("Empty dataset fails without marking completion; retry recovers", async () =>
{
    db.Symbols.RemoveRange(db.Symbols);
    db.JobCompletions.RemoveRange(db.JobCompletions);
    await db.SaveChangesAsync();
    state.LastCheckedDailyTimeUtc = state.LastCheckedWeeklyTimeUtc = null;
    try { await runner.UpdatePricesAsync(default); throw new Exception("Expected failure"); }
    catch (ArgumentNullException) { }
    Assert(state.LastCheckedWeeklyTimeUtc == null && !await db.JobCompletions.AnyAsync() && gate.Gate.CurrentCount == 1, "Failure marked complete or leaked gate");
    db.Symbols.Add(new Symbol("RECOVERED").SetHigh(10));
    await db.SaveChangesAsync();
    await runner.InitializeAsync(default);
    Assert(cache.GetSymbols().Count == 1 && cache.GetSymbols()[0].IsTopNine && await db.JobCompletions.CountAsync() == 2, "Recovery failed");
});
Console.WriteLine($"{passed} checks passed. No application database used.");

public class Stub : DispatchProxy
{
    public Func<MethodInfo, object?[]?, object?> Handler = null!;
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    public static T Create<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = Create<T, Stub>();
        ((Stub)(object)proxy).Handler = handler;
        return proxy;
    }
}
