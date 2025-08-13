using System.Text.Json;
using Haunt4Treasure.Models;
using Microsoft.EntityFrameworkCore;
namespace Haunt4Treasure.Repository;

public interface IAllRepository
{
    Task<UserDetialResponse> GetUserDetials(Guid userId);
    Task<string> AddUserAsync(AddUserRequest accessToken);
    Task<List<Question>> GetQuestionsAsync(GameSession gameSession, Guid? category);
    Task<List<Question>> GetSampleQuestionsAsync();
    Task<bool> UpdateUserAsync(User user);
    Task<List<QuestionCategory>> ProcessSampleQuestionsCategories();
    Task<(User User, decimal balance)> GetUserAsync(string email);
    Task<decimal> TopUpWalletAsync(Guid userId, decimal amount);
    Task<bool> UpdateGameSessionCashoutAsync(GameCashOut cashOut);
    Task<ReturnObject> PostQuestion(List<Question> ques);
    Task<bool> UpdateProfile(Guid userId, string profilePic, string bankName, string accountNumber);
}
public class AllRepository(HauntDbContext dbContext, ILogger<AllRepository> logger) : IAllRepository
{

    private readonly HauntDbContext _dbContext = dbContext;
    private readonly ILogger<AllRepository> _logger = logger;

    #region AuthFlow
    public async Task<string> AddUserAsync(AddUserRequest accessToken)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }
    }

    public async Task<(User User, decimal balance)> GetUserAsync(string email)
    {
        try
        {
            decimal balance = 0;
            var res = await _dbContext.Users.FirstOrDefaultAsync(o => o.Email.ToLower().Equals(email.ToLower()));

            if (res != null)
                balance = await _dbContext.WalletTransactions
                           .Where(t => t.UserId == res.Id)
                           .SumAsync(t => t.Type == "CR" ? t.Amount : -t.Amount);
            return (res, balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(User user)
    {
        try
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }
    }
    #endregion
    #region Question

    public async Task<ReturnObject> PostQuestion(List<Question> ques)
    {
        try
        {
            await _dbContext.Questions.AddRangeAsync(ques);
            await _dbContext.SaveChangesAsync();
            return new ReturnObject { Data = null, Status = true, Message = "Questions added successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }

    }
    public async Task<List<QuestionCategory>> ProcessSampleQuestionsCategories()
    {
        //select all the categories from the questions table but make RANDOM the first Item on the list
        //so select the categories from the questions table and make the first item "RANDOM"
        try
        {
            var res = await _dbContext.QuestionCategory.ToListAsync();
            return res;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }

    }
    public async Task<List<Question>> GetQuestionsAsync(GameSession gameSession, Guid? category)
    {
        try
        {
            // update any existing game session with the same userId and status "InProgress"
            var existingSession = await _dbContext.GameSessions
                .FirstOrDefaultAsync(gs => gs.UserId == gameSession.UserId);
            if (existingSession != null)
            {
                existingSession.Status = "Completed";
                existingSession.EndedAt = DateTime.UtcNow;
                _dbContext.GameSessions.Update(existingSession);
            }
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
                Status = "Completed From Stake",
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
                if (questions.Count < 25)
                {
                    // if the question not up to 25 select new quuestions and add to the questions list must be distinct
                    var additionalQuestions = await _dbContext.Questions
                        .Where(q => !questions.Select(q => q.Text).Contains(q.Text))
                        .GroupBy(q => q.Text)
                        .Select(g => g.First())
                        .OrderBy(q => Guid.NewGuid())
                        .Take(25 - questions.Count)
                        .ToListAsync();

                    questions.AddRange(additionalQuestions);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }

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
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var wallet = await _dbContext.Wallets
                .AsTracking() // Ensure it's tracked for update
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    //  Id = Guid.NewGuid(), // Ensure you assign an ID if using one
                    UserId = userId,
                    Balance = 0
                };
                _dbContext.Wallets.Add(wallet);
            }

            wallet.Balance += amount;

            // Optional: no need to call Update() since EF is tracking already.
            // _dbContext.Wallets.Update(wallet); <-- not needed

            var transactionRecord = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = amount,
                Status = "Completed From Deposit",
                Type = "CR",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.WalletTransactions.Add(transactionRecord);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return wallet.Balance;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "An error occurred while processing request");
            throw new InvalidOperationException("Wallet update failed due to a concurrency conflict.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            await transaction.RollbackAsync();
            throw;
        }
    }


    //to update the gamesession with the cashout amount by the sessionId
    public async Task<bool> UpdateGameSessionCashoutAsync(GameCashOut cashOut)
    {
        try
        {
            var gameSession = await _dbContext.GameSessions.FirstOrDefaultAsync(gs => gs.Id == cashOut.SessionId);
            if (gameSession == null)
            {
                throw new InvalidOperationException("Game session not found.");
            }
            gameSession.CashoutAmount = cashOut.CashoutAmount;
            gameSession.Status = "Completed";
            gameSession.EndedAt = DateTime.UtcNow;
            gameSession.UsedFiftyFifty = cashOut.Fiftyfifty;
            gameSession.UsedSkip = cashOut.Skipped;
            gameSession.NumberOfAnsweredGame = cashOut.NumberOfAnsweredQuestions;
            _dbContext.GameSessions.Update(gameSession);
            //update the wallet transaction
            var walletTransaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                UserId = gameSession.UserId,
                Amount = cashOut.CashoutAmount,
                Type = "CR",
                Status = "Completed From CashOut",
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.WalletTransactions.Add(walletTransaction);
            // Update the user's wallet balance
            var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == gameSession.UserId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found for the user.");
            }
            wallet.Balance += cashOut.CashoutAmount;
            _dbContext.Wallets.Update(wallet);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }

    }
    #endregion
    #region Profile
    //to topup the wallet and update the balance
    public async Task<UserDetialResponse> GetUserDetials(Guid userId)
    {

        try
        {
            var wallet = _dbContext.Users
        .Where(u => u.Id == userId)
        .GroupJoin(
            _dbContext.WithdrawalBanks,
            user => user.Id,
            bank => bank.UserId,
            (user, banks) => new { user, banks }
        )
        .SelectMany(
            ub => ub.banks.DefaultIfEmpty(), // ensures even users without bank info are included
            (ub, bank) => new UserDetialResponse
            {
                Id = ub.user.Id,
                FirstName = ub.user.FirstName,
                LastName = ub.user.LastName,
                Email = ub.user.Email,
                Balance = _dbContext.Wallets
                    .Where(w => w.UserId == ub.user.Id)
                    .Select(w => w.Balance)
                    .FirstOrDefault(),
                ProfileImagePath = ub.user.ProfileImagePath,
                WithdrawalBank = bank == null ? null : new WithdrawalBankResponse
                {
                    BankName = bank.BankName,
                    AccountNumber = bank.AccountNumber
                }
            }
        )
        .FirstOrDefault();

            return wallet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw ex;
        }
    }


    //to update the gamesession with the cashout amount by the sessionId
    public async Task<bool> UpdateProfile(Guid userId, string profilePic, string bankName, string accountNumber)
    {
        try
        {
            // Fetch the user with tracking for updates
            if (!string.IsNullOrEmpty(profilePic))
            {
                var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

                if (user is null)
                    throw new InvalidOperationException("User not found.");

                // Update profile image
                user.ProfileImagePath = profilePic;
                _dbContext.Users.Update(user);
            }

            // Check for existing withdrawal bank details
            var withdrawalBank = await _dbContext.WithdrawalBanks
                .FirstOrDefaultAsync(wb => wb.UserId == userId);

            if (withdrawalBank is null)
            {
                withdrawalBank = new WithdrawalBank
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BankName = bankName,
                    AccountNumber = accountNumber
                };
                await _dbContext.WithdrawalBanks.AddAsync(withdrawalBank);
            }
            else
            {
                withdrawalBank.BankName = bankName;
                withdrawalBank.AccountNumber = accountNumber;
                _dbContext.WithdrawalBanks.Update(withdrawalBank);
                // No need to call Update explicitly due to tracking
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing request");
            throw;
        }

    }

    #endregion
}
