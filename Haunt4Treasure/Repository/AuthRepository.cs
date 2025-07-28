using Haunt4Treasure.Models;
using Microsoft.EntityFrameworkCore;

namespace Haunt4Treasure.Repository;

public interface IAuthRepository
{
    // Define methods for authentication operations
    Task<string> AddUserAsync(AddUserRequest accessToken);
     Task<(User User, decimal balance)> GetUserAsync(string email);
}
public class AuthRepository(HauntDbContext dbContext) : IAuthRepository
{

    private readonly HauntDbContext _dbContext = dbContext;
    public async Task<string> AddUserAsync(AddUserRequest accessToken)
    {
        _dbContext.Users.Add(new User
        {
            FirstName = accessToken.FirstName,
            LastName = accessToken.LastName,
            Email = accessToken.Email,
            PhoneNumber = accessToken.PhoneNumber,
            PasswordHash = accessToken.Password,
            ProfileImagePath = accessToken.ProfileImagePath,
            AgeConfirmed = accessToken.AgeConfirmed,
            IsEmailUser = accessToken.IsEmailUser
        });

        var result = await _dbContext.SaveChangesAsync(); 
        return result.ToString();
    }

    public async Task<(User User, decimal balance)> GetUserAsync(string email)
    {
        decimal balance = 0;
        var res = await _dbContext.Users.FirstOrDefaultAsync(o => o.Email.Equals(email));

        if (res != null)
            balance = await _dbContext.WalletTransactions
                       .Where(t => t.UserId == res.Id)
                       .SumAsync(t => t.Type == "CR" ? t.Amount : -t.Amount);
        return (res,balance);
    }
}
