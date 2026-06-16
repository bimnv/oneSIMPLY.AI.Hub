using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Models;

namespace oneSIMPLY.AIHub.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Subscription> Subscriptions { get; set; } = null!;
    public DbSet<UserSubscription> UserSubscriptions { get; set; } = null!;
    public DbSet<UsageLog> UsageLogs { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<SocialAccount> SocialAccounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("AIHub_Users");
        modelBuilder.Entity<Subscription>().ToTable("AIHub_Subscriptions");
        modelBuilder.Entity<UserSubscription>().ToTable("AIHub_UserSubscriptions");
        modelBuilder.Entity<UsageLog>().ToTable("AIHub_UsageLogs");
        modelBuilder.Entity<Transaction>().ToTable("AIHub_Transactions");
        modelBuilder.Entity<SocialAccount>().ToTable("AIHub_SocialAccounts");

        modelBuilder.Entity<Subscription>().Property(s => s.Price).HasPrecision(18, 2);
        modelBuilder.Entity<Transaction>().Property(t => t.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Subscription>().HasIndex(s => s.Name).IsUnique();
        modelBuilder.Entity<Transaction>().HasIndex(t => t.Code).IsUnique();

        modelBuilder.Entity<UserSubscription>()
            .HasOne(us => us.Subscription)
            .WithMany()
            .HasForeignKey(us => us.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserSubscription>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UsageLog>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SocialAccount>()
            .HasIndex(a => new { a.UserId, a.Platform })
            .IsUnique();

        modelBuilder.Entity<SocialAccount>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
