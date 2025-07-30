using Microsoft.EntityFrameworkCore;

namespace Haunt4Treasure.Models;

public class HauntDbContext : DbContext
{
    public HauntDbContext(DbContextOptions<HauntDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<QuestionCategory> QuestionCategory { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<Withdrawal> Withdrawals { get; set; }
    public DbSet<WithdrawalBank> WithdrawalBanks { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<CustomerUser> CustomerUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<QuestionRawDto>().HasNoKey();
        // Wallet: User (1-to-1)
        modelBuilder.Entity<Wallet>()
            .HasOne(w => w.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // GameSession: User (1-to-many)
        modelBuilder.Entity<GameSession>()
            .HasOne(gs => gs.User)
            .WithMany(u => u.GameSessions)
            .HasForeignKey(gs => gs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WalletTransaction: User (1-to-many)
        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.User)
            .WithMany(u => u.WalletTransactions)
            .HasForeignKey(wt => wt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Withdrawal: User (1-to-many)
        modelBuilder.Entity<Withdrawal>()
            .HasOne(w => w.User)
            .WithMany(u => u.Withdrawals)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WithdrawalBank: User (1-to-many) — restrict to avoid multiple cascade paths
        modelBuilder.Entity<WithdrawalBank>()
            .HasOne(wb => wb.User)
            .WithMany()
            .HasForeignKey(wb => wb.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Withdrawal: WithdrawalBank (1-to-many) — restrict to avoid multiple cascade paths
        modelBuilder.Entity<Withdrawal>()
            .HasOne(w => w.WithdrawalBank)
            .WithMany()
            .HasForeignKey(w => w.WithdrawalBankId)
            .OnDelete(DeleteBehavior.Restrict);

        // CustomerUser: User (1-to-1)
        modelBuilder.Entity<CustomerUser>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
