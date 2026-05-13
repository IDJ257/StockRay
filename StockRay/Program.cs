
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Core;
using RabbitMQ.Client.Logging;
using StockRay.BackGroundJobs.OnStartUpJob;
using StockRay.BackGroundJobs.SetDailyJob;
using StockRay.BackGroundJobs.SetSymbolStateJob;
using StockRay.BackGroundJobs.SetTopNineWeeklyJob;
using StockRay.Database;
using StockRay.Endpoints;
using StockRay.Models;
using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.Register;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace StockRay
{

   



    public class Program
    {
        public static void Main(string[] args)
        {



            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"))
                );



            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            
        


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

            //eventualen interface zaradi testing


            builder.Services.AddScoped<IOnStartUp, OnStartUp>();
            builder.Services.AddSingleton<IFastAccess, FastAccess>();
            builder.Services.AddScoped<ISetDaily, SetDaily>();
            builder.Services.AddScoped<ISetTopNineWeekly, SetTopNineWeekly>();
            builder.Services.AddScoped<ISetSymbolState, SetSymbolState>();
            builder.Services.AddScoped<RegisterService>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<PublicDashboardService>();


            //da se proveri dali moga da injectvam singleton v scoped i vice versa.
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


            //singleton service koito da loadne vsichki simboli

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.MapEndpoints();            

            app.UseAuthorization();


            app.Run();


        }
    }
}
