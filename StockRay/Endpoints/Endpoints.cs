
using StockRay.Services.Register;
namespace StockRay
{

    //Authentication no za sega bez authentication samo password
    //check against db

    public static partial class Endpoints
    {

        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {

            var group = app.MapGroup("api/auth");

            group.MapPost("register", Register);






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

    }








}
