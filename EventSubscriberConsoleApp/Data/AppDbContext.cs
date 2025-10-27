using Microsoft.EntityFrameworkCore;
using EventSubscriberConsoleApp.Models;

namespace EventSubscriberConsoleApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.UserId);
            b.Property(u => u.UserName).IsRequired().HasMaxLength(200);
            b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            b.Property(u => u.Status).HasDefaultValue(UserStatus.Active);
            b.Property(u => u.Email).HasMaxLength(255);
            b.Property(u => u.FirstName).HasMaxLength(100);
            b.Property(u => u.LastName).HasMaxLength(100);
            b.Property(u => u.PhoneNumber).HasMaxLength(50);
            b.Property(u => u.CreatedAt).IsRequired();
        });
    }
}
