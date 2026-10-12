using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using StockRay.Models;
using StockRay.Services.AddSymbol;
using StockRay.Services.GetAllSymbols;
using StockRay.Services.GetSymbol;
using StockRay.Services.Identity;
using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.RemoveSymbol;
using StockRay.Shared;
using System.Security.Claims;

namespace StockRay.Endpoints;

public static class Endpoints
{
    public static void MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Redirect("/PublicDash"));
        app.MapGet("/public", GetPublicDashboard);
        app.MapGet("/antiforgery/token", GetAntiforgeryToken);
        app.MapGet("/account", GetAccount).RequireAuthorization();

        app.MapPost("/register", Register).AddEndpointFilter<AntiforgeryValidationFilter>();
        app.MapPost("/login", Login).AddEndpointFilter<AntiforgeryValidationFilter>();
        app.MapPost("/logout", Logout)
            .RequireAuthorization()
            .AddEndpointFilter<AntiforgeryValidationFilter>();

        app.MapPost("/AddSymbol", AddSymbol)
            .RequireAuthorization()
            .AddEndpointFilter<AntiforgeryValidationFilter>();
        app.MapDelete("/RemoveSymbol", RemoveSymbol)
            .RequireAuthorization()
            .AddEndpointFilter<AntiforgeryValidationFilter>();
        app.MapGet("/GetSymbols", GetSymbols).RequireAuthorization();
        app.MapGet("/GetAllSymbols", GetAllSymbols);
    }

    private static IResult GetAntiforgeryToken(HttpContext context, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(context);
        return Results.Ok(new { requestToken = tokens.RequestToken });
    }

    private static IResult GetAccount() => Results.Ok(new { authenticated = true });

    private static async Task<IResult> Register(
        [FromServices] IdentityAccountService accounts,
        [FromBody] RegisterDto registerDto)
    {
        var result = await accounts.RegisterAsync(registerDto.Name, registerDto.Email, registerDto.Password);
        if (result.Succeeded)
        {
            return Results.Ok();
        }

        var errors = result.Errors
            .GroupBy(error => error.Code)
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        return Results.ValidationProblem(errors);
    }

    private static async Task<IResult> Login(
        [FromServices] IdentityAccountService accounts,
        [FromBody] LoginDto loginDto)
    {
        var result = await accounts.LoginAsync(loginDto.Email, loginDto.Password);

        if (result.Succeeded)
        {
            return Results.Ok();
        }

        if (result.IsLockedOut)
        {
            return Results.Problem(statusCode: StatusCodes.Status423Locked, title: "Account is temporarily locked.");
        }

        return Results.Unauthorized();
    }

    private static async Task<IResult> Logout([FromServices] IdentityAccountService accounts)
    {
        await accounts.LogoutAsync();
        return Results.NoContent();
    }

    private static IResult GetAllSymbols(GetAllSymbolsService service) => Results.Ok(service.GetAllSymbols());

    private static async Task<IResult> GetSymbols(GetSymbolService service, ClaimsPrincipal claims)
    {
        if (!TryGetUserId(claims, out var userId))
        {
            return Results.Forbid();
        }

        var result = await service.GetSymbolsAsync(userId);
        return result.HasPassed ? Results.Ok(result.Value) : Results.BadRequest(result);
    }

    private static async Task<IResult> RemoveSymbol(
        [FromServices] RemoveSymbolService service,
        [FromBody] UserSymbolInboundDto inbound,
        ClaimsPrincipal claims)
    {
        if (!TryGetUserId(claims, out var userId))
        {
            return Results.Forbid();
        }

        var result = await service.RemoveSymbolAsync(userId, inbound);
        return result.HasPassed ? Results.NoContent() : Results.BadRequest(result);
    }

    private static async Task<IResult> AddSymbol(
        AddSymbolService service,
        UserSymbolInboundDto inbound,
        ClaimsPrincipal claims)
    {
        if (!TryGetUserId(claims, out var userId))
        {
            return Results.Forbid();
        }

        var result = await service.AddSymbolAsync(inbound, userId);
        return result.HasPassed ? Results.Ok(result.Value) : Results.BadRequest();
    }

    private static IResult GetPublicDashboard(PublicDashboardService service)
    {
        var result = service.GivePublicDashboard();
        return result.HasPassed ? Results.Ok(result.Value) : Results.BadRequest(result);
    }

    private static bool TryGetUserId(ClaimsPrincipal claims, out int userId) =>
        int.TryParse(claims.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
