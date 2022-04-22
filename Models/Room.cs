using System.ComponentModel.DataAnnotations;

namespace taChat.App.Models;

public class Room
{
    public enum RoomType
    {
        Private,
        Group
    }

    public ulong Id { get; init; }

    public ICollection<Message> Messages { get; } = new HashSet<Message>();

    public ICollection<User> Users { get; init; } = new HashSet<User>();

    [Required, MaxLength(50)]
    public string Name { get; set; } = null!;

    [Required]
    public RoomType Type { get; init; }
}