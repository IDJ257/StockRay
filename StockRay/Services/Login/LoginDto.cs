namespace StockRay.Services.Login
{
    public class LoginDto
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public LoginDto(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }




    }
}