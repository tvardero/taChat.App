namespace taChat.App.Models;

public class Message
{
    public ulong Id { get; init; }

    public User Sender { get; init; } = null!;

    public Chat Chat { get; init; } = null!;

    public string Text { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
