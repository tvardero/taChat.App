using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace taChat.App.Models;

public class User : IdentityUser
{
    public ICollection<Message> Messages { get; } = new HashSet<Message>();

    public ICollection<RoomUser> Chats { get; } = new HashSet<RoomUser>();
}