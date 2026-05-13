using StockRay.Services.Login;
using StockRay.Services.PublicDashboard;
using StockRay.Services.Register;
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
