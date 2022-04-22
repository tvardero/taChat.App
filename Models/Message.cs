using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace taChat.App.Models;

public class Message
{
    public ulong Id { get; init; }

    [HiddenInput]
    public string SenderId { get; init; } = null!;

    [ForeignKey(nameof(SenderId))]
    public User Sender { get; init; } = null!;

    [HiddenInput]
    public ulong RoomId { get; init; }

    [ForeignKey(nameof(RoomId))]
    public Room Room { get; init; } = null!;

    [Required, MaxLength(1000)]
    public string Text { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
