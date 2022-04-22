using Microsoft.AspNetCore.Mvc;

namespace taChat.App.ViewComponents;

public class RoomListViewComponent : ViewComponent
{
    public RoomListViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        IEnumerable<Room> rooms = _context.Rooms;
        return View(rooms);
    }

    private readonly ApplicationDbContext _context;
}