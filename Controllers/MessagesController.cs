using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using taChat.App.Hubs;

namespace taChat.App.Controllers;

[Authorize]
public class MessagesController : Controller
{
    public MessagesController(IHubContext<MessagesHub> messagesHub, ApplicationDbContext context, UserManager<User> userManager)
    {
        _messagesHub = messagesHub;
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> JoinRoom(string connectionId, ulong roomId)
    {
        await _messagesHub.Groups.AddToGroupAsync(connectionId, $"chat{roomId}");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> LeaveRoom(string connectionId, ulong roomId)
    {
        await _messagesHub.Groups.RemoveFromGroupAsync(connectionId, $"chat{roomId}");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string text, ulong roomId)
    {
        if (string.IsNullOrEmpty(text) || roomId == 0) return BadRequest();

        if (await CheckCurrentUserPerkAsync(RoomPerks.Write, roomId))
        {
            Message message = new()
            {
                Timestamp = DateTime.Now,
                SenderId = (await GetCurrentUserAsync()).Id,
                RoomId = roomId,
                Text = text
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            object messageData = new
            {
                time = message.Timestamp.ToShortTimeString(),
                text = message.Text,
                sender = message.Sender.UserName,
                senderId = message.SenderId,
                edited = message.WasEdited,
                messageId = message.Id
            };

            // BUG! Exclude people that are deafen
            await _messagesHub.Clients.Group($"chat{roomId}").SendAsync("getMessage", messageData);
        }
        else
        {
            return Forbid();
        }

        return Ok();
    }

    private async Task<User> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);

    private async Task<bool> CheckCurrentUserPerkAsync(RoomPerks perk, ulong roomId)
    {
        User cu = await GetCurrentUserAsync();
        RoomPerks? cuPerks = (await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == cu.Id && ru.RoomId == roomId))?.Perks;
        return cuPerks != null && (cuPerks & perk) == perk;
    }

    private readonly IHubContext<MessagesHub> _messagesHub;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
}