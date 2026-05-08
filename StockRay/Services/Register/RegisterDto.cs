namespace StockRay
{


    public static partial class Endpoints
    {
        public class RegisterDto
        {
            public string Name { get; set; }

            public string Password { get; set; }

            public string Email { get; set; }

            public RegisterDto(string name, string password, string email)
            {
                Name = name;
                Password = password;
                Email = email;
            }
        }

    }








}
