using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taChat.App.Models;

public class Message
{
    public ulong Id { get; init; }

    public string SenderId { get; init; } = null!;

    [ForeignKey(nameof(SenderId))]
    public User Sender { get; init; } = null!;

    public ulong RoomId { get; init; }

    [ForeignKey(nameof(RoomId))]
    public Room Room { get; init; } = null!;

    [Required, MaxLength(1000)]
    public string Text { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
