using HippoServer.DataModel;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

public class DatabaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Activation> Activations { get; set; } = null!;
    public DbSet<Verification> Verifications { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(account => account.Id);
        modelBuilder.Entity<Account>().HasIndex(account => account.Email).IsUnique();
        
        modelBuilder.Entity<Activation>().HasKey(account => account.Id);
        modelBuilder.Entity<Activation>().HasIndex(activation => activation.Code).IsUnique();
        modelBuilder.Entity<Verification>().HasOne(activation => activation.Account);

        modelBuilder.Entity<Verification>().HasKey(verification => verification.Id);
        modelBuilder.Entity<Verification>().HasIndex(verification => verification.Code).IsUnique();
        modelBuilder.Entity<Verification>().HasOne(verification => verification.Account)
            .WithMany(account => account.Verifications);
    }
}