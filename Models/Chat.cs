namespace taChat.App.Models;

public class Chat
{
    public enum ChatType
    {
        Private,
        Group
    }

    public ulong Id { get; init; }

    public ICollection<Message> Messages { get; } = new HashSet<Message>();

    public ICollection<User> Users { get; init; } = new HashSet<User>();

    public ChatType Type { get; init; }
}