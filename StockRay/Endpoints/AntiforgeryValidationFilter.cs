using Microsoft.AspNetCore.Antiforgery;

namespace StockRay.Endpoints;

public sealed class AntiforgeryValidationFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.BadRequest(new { message = "Invalid antiforgery token." });
        }

        return await next(context);
    }
}
