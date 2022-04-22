using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace taChat.App.Repository;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Room> Rooms { get; init; } = null!;

    public DbSet<Message> Messages { get; init; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Room>()
            .HasMany(r => r.Messages)
            .WithOne(m => m.Room);

        builder.Entity<Room>()
            .HasMany(c => c.Users)
            .WithMany(u => u.Chats);

        builder.Entity<User>()
            .HasMany(u => u.Messages)
            .WithOne(m => m.Sender);
    }
}