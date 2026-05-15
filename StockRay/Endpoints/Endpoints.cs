using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using StockRay.BackGroundJobs;
using StockRay.Models;
using StockRay.Services.AddSymbol;
using StockRay.Services.GetSymbol;
using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.Register;
using StockRay.Services.RemoveSymbol;
using StockRay.Shared;
namespace StockRay.Endpoints
{

    //Authentication no za sega bez authentication samo password
    //check against db

    public static class Endpoints
    {

        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {

            //var group = app.MapGroup("api/");

            //group.MapPost("register", Register);

            //group.MapPost("login", Login);

            app.MapGet("/public", GetPublicDashboard);


            app.MapPost("/register", Register);

            app.MapPost("/login", Login);

            //id = userId
            app.MapPost("/AddSymbol/{id}", AddSymbol);

            app.MapPost("/RemoveSymbol/{id}", RemoveSymbol);


            app.MapGet("/GetSymbols/{id}", GetSymbols);






        }



        public static async Task<IResult> GetSymbols(
          [FromRoute] int id,
          GetSymbolService getSymbolService


          )
        {

            var res = await getSymbolService.GetSymbolsAsync(id);

            return res.HasPassed ? Results.Ok(res.Value) : Results.BadRequest(res);

        }


        public static async Task<IResult> RemoveSymbol(
            [FromRoute] int id,
            RemoveSymbolService removeSymbolService,
            UserSymbolInboundDto userSymbolInbound

            )
        {

            var res = await removeSymbolService.RemoveSymbolAsync(id, userSymbolInbound);

            return res.HasPassed ? Results.Ok() : Results.BadRequest(res);

        }


        public static async Task<IResult> AddSymbol(
            [FromRoute] int id,
            AddSymbolService addSymbolService,
            UserSymbolInboundDto userSymbolInbound
            )
        {

            var res = await addSymbolService.AddSymbolAsync(userSymbolInbound, id);

            return res.HasPassed ? Results.Ok(res.Value) : Results.BadRequest();

        }



        public static async Task<IResult> Register(
            RegisterService registerService,
            RegisterDto registerDto

            )
        {
            //DTO
            var res = await registerService.RegisterAsync(registerDto.Name, registerDto.Password, registerDto.Email);

            return res.HasPassed ? Results.Ok() : Results.BadRequest(res);


        }


        public static async Task<IResult> Login(
            LoginService loginService,
            LoginDto loginDto

            )
        {
            //Moje bi trqq vrushtame usera
            var res = await loginService.LoginAsync(loginDto.UserName, loginDto.Password);


            return res.HasPassed ? Results.Ok() : Results.BadRequest(res);
        }

        public static IResult GetPublicDashboard(
            PublicDashboardService publicDashBoardService
            )
        {


            var res = publicDashBoardService.GivePublicDashboard();


            return res.HasPassed ? Results.Ok(res.Value) : Results.BadRequest(res);
        }

    }











}
