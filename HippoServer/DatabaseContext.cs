using HippoServer.DataModel;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

public class DatabaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Activation> Activations { get; set; } = null!;
    public DbSet<Verification> Verifications { get; set; } = null!;
    
    public DbSet<Group> Groups { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;

    public DbSet<Event> Events { get; set; } = null!;
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>().HasKey(account => account.Id);
        modelBuilder.Entity<Account>().HasIndex(account => account.Email).IsUnique();
        modelBuilder.Entity<Account>().HasMany(account => account.Verifications)
            .WithOne(verification => verification.Account)
            .HasForeignKey(verification => verification.AccountId);
        modelBuilder.Entity<Account>().HasMany(account => account.Permissions)
            .WithOne(permission => permission.Account)
            .HasForeignKey(permission => permission.AccountId);

        modelBuilder.Entity<Activation>().HasKey(activation => activation.Id);
        modelBuilder.Entity<Activation>().HasIndex(activation => activation.Code).IsUnique();
        modelBuilder.Entity<Verification>().HasOne(activation => activation.Account);

        modelBuilder.Entity<Verification>().HasKey(verification => verification.Id);
        modelBuilder.Entity<Verification>().HasIndex(verification => verification.Code).IsUnique();
        
        modelBuilder.Entity<Group>().HasKey(group => group.Id);
        modelBuilder.Entity<Group>().HasIndex(group => group.Name).IsUnique();
        modelBuilder.Entity<Group>().HasMany(group => group.Permissions)
            .WithOne(permission => permission.Group)
            .HasForeignKey(permission => permission.GroupId);

        modelBuilder.Entity<Permission>().HasKey(permission => permission.Id);
        
        modelBuilder.Entity<Event>().HasKey(@event => @event.Id);
    }
}