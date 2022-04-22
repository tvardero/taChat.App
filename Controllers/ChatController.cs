using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace taChat.App.Controllers;

[Authorize]
public class ChatController : Controller
{
    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Room room = new() { Name = name, Type = Room.RoomType.Group };
            await _context.Rooms.AddAsync(room);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewRoom), new { roomId = room.Id });
        }

        return Redirect("/");
    }

    public async Task<IActionResult> ViewRoom(ulong roomId)
    {
        var loadedRooms = _context.Rooms
            .Include(r => r.Messages)
            .ThenInclude(m => m.Sender);

        Room room = loadedRooms.SingleOrDefault(r => r.Id == roomId) ?? loadedRooms.First();
        User user = await _context.Users.FirstOrDefaultAsync() ?? new() { UserName = "Hanna" };

        RoomViewModel model = new() { Room = room, UserId = user.Id };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(Message message)
    {
        if (ModelState.IsValid)
        {
            message.Timestamp = DateTime.UtcNow;

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(ViewRoom), new { roomId = message.RoomId });
    }

    private readonly ApplicationDbContext _context;
}