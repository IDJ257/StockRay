using Microsoft.AspNetCore.Identity;
using StockRay.Models;

namespace StockRay.Services.Identity;

public sealed class IdentityAccountService(
    UserManager<User> userManager,
    SignInManager<User> signInManager)
{
    public async Task<IdentityResult> RegisterAsync(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidInput",
                Description = "Name, email, and password are required."
            });
        }

        var user = new User(name.Trim(), email.Trim());
        return await userManager.CreateAsync(user, password);
    }

    public async Task<SignInResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
        {
            return SignInResult.Failed;
        }

        var user = await userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            return SignInResult.Failed;
        }

        return await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: false,
            lockoutOnFailure: true);
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();
}
