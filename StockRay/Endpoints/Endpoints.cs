
using StockRay.Services.Login;
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

            group.MapPost("login", Login);

       






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

            var res = await loginService.LoginAsync(loginDto.UserName, loginDto.Password);


            return res.HasPassed ? Results.Ok() : Results.BadRequest(res);
        }

    }








}
