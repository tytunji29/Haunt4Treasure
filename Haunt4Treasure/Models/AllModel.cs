using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Haunt4Treasure;

// User Entity
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
    public bool AgeConfirmed { get; set; }
    public bool IsEmailUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Wallet Wallet { get; set; }
    [NotMapped]
    public WithdrawalBank WithdrawalBank { get; set; }

    public ICollection<GameSession> GameSessions { get; set; }
    public ICollection<WalletTransaction> WalletTransactions { get; set; }
    public ICollection<Withdrawal> Withdrawals { get; set; }
}

// Question Entity
public class Question
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
}

// GameSession Entity
public class GameSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public decimal AmountStaked { get; set; }
    public string Status { get; set; } = "InProgress";
    public bool UsedSkip { get; set; }
    public bool UsedFiftyFifty { get; set; }
    public decimal? CashoutAmount { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public int NumberOfAnsweredGame { get; set; }
}
// Wallet Entity
public class Wallet
{
    [Key]
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public User User { get; set; }
}

public class QuestionCategory
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string ShortDescription { get; set; }
    public string ImageUrl { get; set; }
}
// WalletTransaction Entity
public class WalletTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } // Deposit, Stake, Win, Cashout
    public string Status { get; set; } // Pending, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Withdrawal Entity
public class Withdrawal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WithdrawalBankId { get; set; }
    public User User { get; set; }
    public WithdrawalBank WithdrawalBank { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// WithdrawalBank Entity
public class WithdrawalBank
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// AdminUser Entity
public class AdminUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
// CustomerUser Entity
public class CustomerUser
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

