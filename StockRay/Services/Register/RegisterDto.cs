namespace StockRay
{


    
        public class RegisterDto
        {
            public string UserName { get; set; }

            public string Password { get; set; }

            public string Email { get; set; }

            public RegisterDto(string userName, string password, string email)
            {
                UserName = userName;
                Password = password;
                Email = email;
            }
        

    }








}
