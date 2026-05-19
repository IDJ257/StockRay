
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Core;
using RabbitMQ.Client.Logging;
using StockRay.BackGroundJobs;
using StockRay.BackGroundJobs.OnStartUpJob;
using StockRay.BackGroundJobs.SetDailyJob;
using StockRay.BackGroundJobs.SetSymbolStateJob;
using StockRay.BackGroundJobs.SetTopNineWeeklyJob;
using StockRay.Database;
using StockRay.Endpoints;
using StockRay.Models;
using StockRay.Other;
using StockRay.Services.AddSymbol;
using StockRay.Services.GetAllSymbols;
using StockRay.Services.GetSymbol;
using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.Register;
using StockRay.Services.RemoveSymbol;
using StockRay.SignalHub;
using System;
using System.Text;


namespace StockRay
{


    public class Program
    {
        public static void Main(string[] args)
        {



            var builder = WebApplication.CreateBuilder(args);


            var provider = builder.Configuration["DataBaseProvider"];

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (provider == "Sqlite")
                {
                    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
                }
                else
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
                }

            });
                



            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();


            builder.Services.AddAuthorization();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwt =>
                {
                    jwt.RequireHttpsMetadata = false;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        //HARDCODED SYMMETRYCSECKEY Just so it runs on multiple machines 
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])),
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        ClockSkew = TimeSpan.Zero
                    };

                });

            builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
            builder.Services.AddQuartz(options =>
            {

                var dailyJobKey = JobKey.Create(nameof(SetDailyJob));

                options.AddJob<SetDailyJob>(dailyJobKey)
                .AddTrigger(
                    tr => tr.ForJob(dailyJobKey)
                    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(6, 0)));





                var topNinejobKey = JobKey.Create(nameof(SetTopNineWeeklyJob));
                options.AddJob<SetTopNineWeeklyJob>(topNinejobKey)
                .AddTrigger(
                    tr => tr.ForJob(topNinejobKey)
                    .WithSchedule(
                        CronScheduleBuilder.WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 6, 0)


                    ));


                var setSymbolSateJobKey = JobKey.Create(nameof(SetSymbolStateJob));

                options.AddJob<SetSymbolStateJob>(setSymbolSateJobKey)
                .AddTrigger(
                    tr => tr.ForJob(setSymbolSateJobKey)
                    .WithSimpleSchedule(s =>
                    s.WithIntervalInSeconds(30)
                    .WithMisfireHandlingInstructionFireNow()
                    .RepeatForever()).StartAt(DateTime.UtcNow.AddSeconds(30)));


                var startUpJobKey = JobKey.Create(nameof(OnStartUpJob));
                options.AddJob<OnStartUpJob>(startUpJobKey)
                .AddTrigger(
                    tr => tr.ForJob(startUpJobKey)
                    .WithSimpleSchedule());

            });


            builder.Services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });



            builder.Services.AddScoped<IOnStartUp, OnStartUp>();
            builder.Services.AddSingleton<IFastAccess, FastAccess>();
            builder.Services.AddSingleton<IActiveGroup, ActiveGroup>();
            builder.Services.AddScoped<ISetDaily, SetDaily>();
            builder.Services.AddScoped<ISetTopNineWeekly, SetTopNineWeekly>();
            builder.Services.AddScoped<ISetSymbolState, SetSymbolState>();
            builder.Services.AddSingleton<TokenProvider>();
            builder.Services.AddScoped<RegisterService>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<PublicDashboardService>();
            builder.Services.AddScoped<RemoveSymbolService>();
            builder.Services.AddScoped<AddSymbolService>();
            builder.Services.AddScoped<GetSymbolService>();
            builder.Services.AddScoped<GetAllSymbolsService>();



            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();




            var app = builder.Build();

            

            //runva se ako sluchaino nqma DB
            //AKo ima Db EF ne izpulnqva Migrate();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                db.Database.Migrate();

                if (!db.Symbols.Any())
                {
                    var seed = SymbolSeed.Seed();
                    db.Symbols.AddRange(seed);
                    db.SaveChanges();
                }
            }



            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.MapEndpoints();


            app.MapHub<SymbolNotifHub>("/sym-notif");




            app.Run();


        }
    }
}
