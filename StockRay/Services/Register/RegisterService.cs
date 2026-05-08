using Microsoft.AspNetCore.Identity;
using StockRay.Database;
using StockRay.Models;
using StockRay.Other;

namespace StockRay.Services.Register
{
    public class RegisterService
    {
        private readonly ApplicationDbContext _context;

        private readonly IPasswordHasher<User> _hasher;

        public RegisterService(
            ApplicationDbContext context,
            IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }


        public async Task<ServiceResult> RegisterAsync(string name, string password, string email)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new FormatException("No password provided");
                }

                var hashedPass = _hasher.HashPassword(null!, password);

                var user = new User(name, hashedPass, email);


                await _context.SaveChangesAsync();

                return new ServiceResult(true);

            }
            catch (Exception ex)
            {

                return new ServiceResult(false, ex.Message);
            }


        }
    }








}
