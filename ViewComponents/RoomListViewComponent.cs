using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace taChat.App.ViewComponents;

public class RoomListViewComponent : ViewComponent
{
    public RoomListViewComponent(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        List<(string name, ulong id)> model = new();

        User cu = await _userManager.GetUserAsync(HttpContext.User);
        IEnumerable<Room> rooms = _context.Rooms
            .Include(r => r.Users)
            .ThenInclude(ru => ru.User)
            .Where(r => r.Users.Any(ru => ru.UserId == cu.Id));

        foreach (Room room in rooms)
        {
            if (room.IsGroup)
            {
                model.Add((room.Name, room.Id));
            }
            else
            {
                string name = room.Users.Single(ru => ru.UserId != cu.Id).User.UserName;
                model.Add((name, room.Id));
            }
        }

        return View(model.AsEnumerable());
    }

    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
}