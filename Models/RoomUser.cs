using System.ComponentModel.DataAnnotations.Schema;

namespace taChat.App.Models;

public class RoomUser
{
    public ulong RoomId { get; init; }

    [ForeignKey(nameof(RoomId))]
    public Room Room { get; init; } = null!;


    public string UserId { get; init; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; init; } = null!;

    public RoomPerks Perks { get; set; } = RoomPerks.Default;
}
