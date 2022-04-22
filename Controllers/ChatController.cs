using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace taChat.App.Controllers;

[Authorize]
public class ChatController : Controller
{
    public ChatController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    public async Task<IActionResult> ViewRoom(ulong roomId = 0)
    {
        Room? room = await _context.Rooms
            .Include(r => r.Messages)
            .ThenInclude(m => m.Sender)
            .SingleOrDefaultAsync(r => r.Id == roomId);

        User user = await _userManager.GetUserAsync(HttpContext.User);

        if (user == null)
        {
            user = new() { UserName = "DevTest" };
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        RoomViewModel model = new() { Room = room, CurrentUserId = user.Id };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(Message message)
    {
        ModelState!.Remove(nameof(Message.Room));
        ModelState!.Remove(nameof(Message.Sender));

        if (ModelState.IsValid)
        {
            message.Timestamp = DateTime.UtcNow;

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(ViewRoom), new { roomId = message.RoomId });
    }

    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
}