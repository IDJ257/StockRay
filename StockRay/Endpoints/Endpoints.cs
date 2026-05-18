using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using StockRay.BackGroundJobs;
using StockRay.Models;
using StockRay.Other;
using StockRay.Services.AddSymbol;
using StockRay.Services.GetAllSymbols;
using StockRay.Services.GetSymbol;
using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.Register;
using StockRay.Services.RemoveSymbol;
using StockRay.Shared;
using StockRay.SignalHub;
using System.Security.Claims;
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
            app.MapPost("/AddSymbol/{id}", AddSymbol).RequireAuthorization();

            app.MapPost("/RemoveSymbol/{id}", RemoveSymbol).RequireAuthorization();


            app.MapGet("/GetSymbols", GetSymbols).RequireAuthorization();

            app.MapGet("/GetAllSymbols", GetAllSymbols);






        }


        public static IResult GetAllSymbols(
         GetAllSymbolsService getAllSymbolsService

         )
        {
            //tva e samo workaround. Nqq da e null sigurno, ama samo za da raboti inache nie s React-a sh opravim neshtat



            var res = getAllSymbolsService.GetAllSymbols();
            //return res.HasPassed ? Results.Ok(res.Value) : Results.BadRequest(res);

            return Results.Ok(res);
        }

        public static async Task<IResult> GetSymbols(
          GetSymbolService getSymbolService,
          ClaimsPrincipal claims

          )
        {
            //tva e samo workaround. Nqq da e null sigurno, ama samo za da raboti inache nie s React-a sh opravim neshtat

            var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) return Results.Forbid();

            var res = await getSymbolService.GetSymbolsAsync(int.Parse(userId));
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
            var res = await loginService.LoginAsync(loginDto.Email, loginDto.Password);
            
            
            return res.HasPassed ? Results.Ok(new { res.Value }) : Results.BadRequest(res.Value);
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
