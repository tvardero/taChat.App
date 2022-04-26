using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taChat.App.Models;

public class Room
{
    public ulong Id { get; init; }

    public ICollection<Message> Messages { get; } = new HashSet<Message>();

    public ICollection<RoomUser> Users { get; init; } = new HashSet<RoomUser>();

    [Required, MaxLength(30)]
    public string Name { get; set; } = null!;

    public bool IsGroup { get; set; }

    public RoomPerks NewUserPerks { get; set; } = RoomPerks.Default;
}