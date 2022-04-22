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

    public string Name { get; set; } = null!;

    public RoomType Type { get; init; }
}