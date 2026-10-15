StockRay Description
StockRay is a web app still in development. The app's main purpose is to allow users to look at numerous stock symbols and add some of them to their private dashboard so they can watch
the symbol's constant changing properties like the price they opened to, the current price and how high and low the price went.
##



## Features

📶 Using [SignalR](https://dotnet.microsoft.com/en-us/apps/aspnet/signalr) for providing real-time updates of each stock symbol

🔄 Using [Quartz.NET](https://www.quartz-scheduler.net/) for scheduling background jobs

🗃️ Database access using Entity Framework Core (SQL Server / SQLite depending on configuration)

💻 Using typescript + React for front-end design.

##

## Requirements
- .NET 9 SDK
- .Node.JS

- Visual Studio 2022

Optional:
- EntityFramework CLI tools


##

# Installation

The frontend now uses React and TypeScript. Before running or publishing the
ASP.NET app, build it from `client` with `npm.cmd install` followed by
`npm.cmd run build`. See [client/README.md](client/README.md) for the build
workflow, preserved routes, SignalR lifecycle, and manual verification checklist.

Frontend source lives entirely in `client/`: React and TypeScript in `client/src/`,
CSS in `client/public/spa/Styles/`, and images in `client/public/spa/images/`.
The frontend build deploys generated HTML and assets to `StockRay/wwwRoot/`
for ASP.NET to serve. Edit frontend files in `client/` and rebuild.

## Using git Clone and CLI tools(.NET9, EntityFramework CLI)
```bash
git clone <https://github.com/IDJ257/StockRay.git> <FolderOfChoice>
cd FolderOfChoice
cd StockRay
cd StockRay/client
npm run CI
npm run build
dotnet ef migrations add Init
dotnet ef database update
dotnet run 
```
Will be available at https://localHost:7165

## Using .ZIP
1. Downloading the ZIP
2. Extract project
3. Open StockRay.sln in Visual Studio 2022
4. Tools → NuGet Package Manager → Package Manager Console
5. Add-Migration Init
6. Update-Database
7. Run
8. https://localHost:7165

# Notes

> **_NOTE:_** Migrations are not pre-generated to avoid conflicts between SQL Server and SQLite development environments.

> **_NOTE:_** The database is generated locally per machine.

> **_NOTE:_**  Quartz jobs run automatically after startup (background processing every 30 seconds and scheduled tasks).

> **_NOTE:_** Apply the `TrackJobCompletions` database migration before running
> the updated background jobs. See [background job recovery](StockRay/BackGroundJobs/README.md)
> for the completion records, first-start behavior, timezone, and recovery order.

> **_NOTE:_**  Stock data is calculated in-app. There is no real stock data yet.

