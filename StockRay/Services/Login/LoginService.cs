using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using StockRay.Database;
using StockRay.Models;
using StockRay.Other;

namespace StockRay.Services.Login
{
    public class LoginService
    {
        private readonly IPasswordHasher<User> _hasher;

        private readonly ApplicationDbContext _context;

        private readonly TokenProvider _tokenProvider;


        public LoginService(IPasswordHasher<User> hasher, ApplicationDbContext context, TokenProvider tokenProvider)
        {
            _hasher = hasher;
            _context = context;
            _tokenProvider = tokenProvider;
        }

        //tova e butaforno realno shoto nakraq shte slagam AUTH/AUTHR 
        public async Task<ServiceResult<string>> LoginAsync(string userName, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName.Equals(userName));

                if (user == null)
                {
                    return new ServiceResult<string>(true);

                }


                var verifRes = _hasher.VerifyHashedPassword(user, user.Password, password);

                if (PasswordVerificationResult.Failed == verifRes)
                {
                    //ne trqq se hvurlq exception poprincip 
                    //no trqqbva da se napravqt custom errori koito da sa constantni a ne magic strings
                    throw new ArgumentException("Passwords don't match");
                }

                string token = _tokenProvider.Create(user);


                return new ServiceResult<string>(true, token);


            }
            catch (Exception ex)
            {

                return new ServiceResult<string>(false, ex.Message);
            }
        }


    }
}