
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockRay.Database;
using StockRay.Models;
using StockRay.Services.Login;
using StockRay.Services.Register;

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

            //eventualen interface zaradi testing
           
            builder.Services.AddScoped<RegisterService>();
            builder.Services.AddScoped<LoginService>();

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

            app.UseAuthorization();
            

            app.Run();
        }
    }
}
