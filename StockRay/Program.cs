
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
using StockRay.Services.AddSymbol;
using StockRay.Services.GetSymbol;
using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.Register;
using StockRay.Services.RemoveSymbol;
using StockRay.SignalHub;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using StockRay.Other;
using Microsoft.AspNetCore.Rewrite;
using StockRay.Services.GetAllSymbols;


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
            

            builder.Services.AddAuthorization();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwt =>
                {
                    jwt.RequireHttpsMetadata = false;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
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

            //eventualen interface zaradi testing


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
            builder.Services.AddScoped<AddSymbolService>();
            builder.Services.AddScoped<RemoveSymbolService>();
            builder.Services.AddScoped<GetSymbolService>();
            builder.Services.AddScoped<GetAllSymbolsService>();




            //da se proveri dali moga da injectvam singleton v scoped i vice versa.
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();


            //singleton service koito da loadne vsichki simboli

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseAuthentication();

           // app.UseAuthorization();
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
