using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace taChat.App.Repository;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Room> Rooms { get; init; } = null!;

    public DbSet<Message> Messages { get; init; } = null!;

    public DbSet<RoomUser> RoomUsers { get; init; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var room = builder.Entity<Room>();
        room.HasMany(r => r.Messages).WithOne(m => m.Room);
        room.HasMany(r => r.Users).WithOne(ru => ru.Room);

        var user = builder.Entity<User>();
        user.HasMany(u => u.Messages).WithOne(m => m.Sender);
        user.HasMany(u => u.Chats).WithOne(ru => ru.User);

        var roomuser = builder.Entity<RoomUser>();
        roomuser.HasKey(ru => new { ru.RoomId, ru.UserId });
    }
}