using Microsoft.AspNetCore.Identity;

namespace taChat.App.Models;

public class User : IdentityUser
{
    public ICollection<Message> Messages { get; } = new HashSet<Message>();

    public ICollection<Room> Chats { get; } = new HashSet<Room>();
}