using Haunt4Treasure.Models;
using Microsoft.EntityFrameworkCore;

namespace Haunt4Treasure.Repository;

public interface IAllRepository
{
    // Define methods for authentication operations
    Task<string> AddUserAsync(AddUserRequest accessToken);
    Task<List<Question>> GetQuestionsAsync(GameSession gameSession, Guid? category);
    Task<List<Question>> GetSampleQuestionsAsync();
    Task<bool> UpdateUserAsync(User user);
    Task<List<QuestionCategory>> ProcessSampleQuestionsCategories();
    Task<(User User, decimal balance)> GetUserAsync(string email);
    Task<decimal> TopUpWalletAsync(Guid userId, decimal amount);
    Task<bool> UpdateGameSessionCashoutAsync(GameCashOut cashOut);
}
public class AllRepository(HauntDbContext dbContext) : IAllRepository
{

    private readonly HauntDbContext _dbContext = dbContext;

    #region AuthFlow
    public async Task<string> AddUserAsync(AddUserRequest accessToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = accessToken.FirstName,
            LastName = accessToken.LastName,
            Email = accessToken.Email,
            PhoneNumber = accessToken.PhoneNumber,
            PasswordHash = accessToken.Password,
            PasswordSalt = accessToken.PasswordSalt,
            ProfileImagePath = accessToken.ProfileImagePath,
            AgeConfirmed = accessToken.AgeConfirmed,
            IsEmailUser = accessToken.IsEmailUser
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user.Id.ToString();
    }

    public async Task<(User User, decimal balance)> GetUserAsync(string email)
    {
        decimal balance = 0;
        var res = await _dbContext.Users.FirstOrDefaultAsync(o => o.Email.Equals(email));

        if (res != null)
            balance = await _dbContext.WalletTransactions
                       .Where(t => t.UserId == res.Id)
                       .SumAsync(t => t.Type == "CR" ? t.Amount : -t.Amount);
        return (res, balance);
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    #endregion
    #region Question
    public async Task<List<QuestionCategory>> ProcessSampleQuestionsCategories()
    {
        //select all the categories from the questions table but make RANDOM the first Item on the list
        //so select the categories from the questions table and make the first item "RANDOM"
        var res = await _dbContext.QuestionCategory.ToListAsync();
        return res;
    }
    public async Task<List<Question>> GetQuestionsAsync(GameSession gameSession, Guid? category)
    {
        // update any existing game session with the same userId and status "InProgress"
        var existingSession = await _dbContext.GameSessions
            .FirstOrDefaultAsync(gs => gs.UserId == gameSession.UserId);
        existingSession.Status = "Completed";
        existingSession.EndedAt = DateTime.UtcNow;
        _dbContext.GameSessions.Update(existingSession);

        // Add the game session and update user's wallet balance in a single SaveChangesAsync call
        _dbContext.GameSessions.Add(gameSession);

        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(u => u.UserId == gameSession.UserId);
        if (wallet != null)
        {
            if (wallet.Balance < gameSession.AmountStaked)
            {
                throw new InvalidOperationException("Insufficient wallet balance.");
            }
            wallet.Balance -= gameSession.AmountStaked;
            _dbContext.Wallets.Update(wallet);
        }

        // Add wallet transaction
        var walletTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = gameSession.UserId,
            Amount = gameSession.AmountStaked,
            Type = "DR",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.WalletTransactions.Add(walletTransaction);

        // Save all changes in one call
        await _dbContext.SaveChangesAsync();
        var questions = new List<Question>();
        if (category.HasValue && category.Value != Guid.Empty)
        {
            var catName = await _dbContext.QuestionCategory
                .Where(c => c.Id == category.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
            // Get 25 random, unique questions by text for the specified category
            questions = await _dbContext.Questions
               .Where(q => q.Category.Contains(catName))
               .GroupBy(q => q.Text)
               .Select(g => g.First())
               .OrderBy(q => Guid.NewGuid())
               .Take(25)
               .ToListAsync();
            if(questions.Count < 25)
            {
                // if the question not up to 25 select new quuestions and add to the questions list must be distinct
                var additionalQuestions = await _dbContext.Questions
                    .Where(q => !questions.Select(q => q.Text).Contains(q.Text))
                    .GroupBy(q => q.Text)
                    .Select(g => g.First())
                    .OrderBy(q => Guid.NewGuid())
                    .Take(25 - questions.Count)
                    .ToListAsync();
            }
            return questions;
        }
        // Get 25 random, unique questions by text
        questions = await _dbContext.Questions
           .GroupBy(q => q.Text)
           .Select(g => g.First())
           .OrderBy(q => Guid.NewGuid())
           .Take(25)
           .ToListAsync();

        return questions;
    }
    public async Task<List<Question>> GetSampleQuestionsAsync()
    {
        var questions = await _dbContext.Questions
           .Where(q => q.Difficulty.ToLower() == "easy")
           .GroupBy(q => q.Text)
           .Select(g => g.First())
           .OrderBy(q => Guid.NewGuid())
           .Take(5)
           .ToListAsync();

        return questions;
    }
    #endregion

    #region Payment
    //to topup the wallet and update the balance
    public async Task<decimal> TopUpWalletAsync(Guid userId, decimal amount)
    {
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new Wallet { UserId = userId, Balance = 0 };
            _dbContext.Wallets.Add(wallet);
        }
        wallet.Balance += amount;
        _dbContext.Wallets.Update(wallet);
        var transaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Type = "CR",
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.WalletTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
        return wallet.Balance;
    }

    //to update the gamesession with the cashout amount by the sessionId
    public async Task<bool> UpdateGameSessionCashoutAsync(GameCashOut cashOut)
    {
        var gameSession = await _dbContext.GameSessions.FirstOrDefaultAsync(gs => gs.Id == cashOut.sessionId);
        if (gameSession == null)
        {
            throw new InvalidOperationException("Game session not found.");
        }
        gameSession.CashoutAmount = cashOut.cashoutAmount;
        gameSession.Status = "Completed";
        gameSession.EndedAt = DateTime.UtcNow;
        gameSession.UsedFiftyFifty = cashOut.fiftyfifty;
        gameSession.UsedSkip = cashOut.skipped;
        gameSession.NumberOfAnsweredGame = cashOut.numberOfAnsweredQuestions;
        _dbContext.GameSessions.Update(gameSession);
        // Update the user's wallet balance
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == gameSession.UserId);
        if (wallet == null)
        {
            throw new InvalidOperationException("Wallet not found for the user.");
        }
        wallet.Balance += cashOut.cashoutAmount;
        _dbContext.Wallets.Update(wallet);
        await _dbContext.SaveChangesAsync();
        return true;
    }
    #endregion
}
