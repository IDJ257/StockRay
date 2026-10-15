# Background job checks

Run `dotnet run --project checks/BackgroundJobs` from the repository root.

This executable harness references the application directly, uses an isolated in-memory SQLite database, and substitutes SignalR clients and Quartz execution contexts. It requires no additional test packages. Assertions fail the process with a nonzero exit code. It is not discovered by `dotnet test`.

Checks cover schedule boundaries (including Sofia daylight-saving changes), startup cache population, Top Nine selection, daily reset values, persisted completion records, price bounds, notification dispatch, duplicate schedule checks, lock release after errors, cancelled lock waits, Quartz wrapper dispatch/error propagation, and empty-dataset recovery.

One check deliberately reproduces a limitation: a pending SignalR send holds the shared gate, blocks other jobs, and does not respond to the running job's cancellation token. A passing reproduction confirms the limitation is present; it does not indicate that behavior is desirable.

These checks do not validate live SQL Server, deployed migrations/configuration, actual Quartz trigger execution, network delivery to browsers, or multiple application instances. Application source is unchanged.
