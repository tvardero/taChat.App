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

    public async Task<IActionResult> CreateRoom(string name, bool isGroup)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Room r = new() { Name = name, IsGroup = isGroup };
            await _context.Rooms.AddAsync(r);

            User cu = await GetCurrentUserAsync();
            RoomUser ru = new() { Room = r, UserId = cu.Id, Perks = isGroup ? RoomPerks.Creator : RoomPerks.PrivateChat };
            await _context.RoomUsers.AddAsync(ru);

            if (!isGroup)
            {
                User? su = await _userManager.FindByNameAsync(name);
                if (su == null) return NotFound(); // TODO: Prettify

                // TODO: 1. Check that you are not banned on second user account
                // TODO: 2. Do not create new private chat with the same user twice

                RoomUser sru = new() { Room = r, User = su, Perks = RoomPerks.PrivateChat };
                await _context.RoomUsers.AddAsync(sru);
            }

            await _context.SaveChangesAsync();

            // TODO: push update for rooms list for every involved user

            return RedirectToAction(nameof(ViewRoom), new { roomId = r.Id });
        }

        return Redirect("/");
    }

    [HttpPost]
    public async Task<IActionResult> LeaveRoom(ulong roomId)
    {
        User cu = await GetCurrentUserAsync();
        RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == cu.Id && ru.RoomId == roomId);

        if (ru != null)
        {
            _context.RoomUsers.Remove(ru);
            await _context.SaveChangesAsync();

            // TODO: push update for all users that were in manageRoom page atm
        }

        return RedirectToAction(nameof(ViewRoom));
    }

    [HttpGet]
    public async Task<IActionResult> ViewRoom(ulong roomId = 0)
    {
        RoomViewModel model = new();

        ViewData["roomName"] = null;
        ViewData["roomId"] = null;
        ViewData["managePage"] = false;

        if (roomId != 0)
        {
            User cu = await GetCurrentUserAsync();

            RoomUser? ru = await _context.RoomUsers
                .Include(ru => ru.Room)
                .ThenInclude(r => r.Messages)
                .ThenInclude(m => m.Sender)
                .SingleOrDefaultAsync(ru => ru.User.Id == cu.Id && ru.Room.Id == roomId);
            if (ru != null)
            {
                ViewData["roomName"] = ru.Room.IsGroup ? ru.Room.Name : "Private chat";
                ViewData["roomId"] = ru.Room.Id;

                model = new() { Room = ru.Room, Perks = ru.Perks, CurrentUserId = cu.Id };

                return View(model);
            }
            return RedirectToAction(nameof(ViewRoom));
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string text, ulong roomId)
    {
        if (ModelState.IsValid)
        {
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

                // TODO: push update for users that are currenty in this chat
            }
            else
            {
                return Forbid(); // TODO: Prettify
            }
        }

        return RedirectToAction(nameof(ViewRoom), new { roomId });
    }

    [HttpGet]
    public async Task<IActionResult> EditMessage(ulong messageId, ulong roomId)
    {
        Message? message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == messageId);

        if (message != null)
        {
            TempData["edit"] = true;
            TempData["editMessageId"] = messageId.ToString();
            TempData["editMessageText"] = message.Text;
        }

        return RedirectToAction(nameof(ViewRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> EditMessage(string text, ulong messageId, ulong roomId)
    {
        Message? message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == messageId);
        if (message != null && text != null)
        {
            message.Text = text;
            message.Timestamp = DateTime.Now;
            message.WasEdited = true;

            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ViewRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteMessage(ulong messageId, ulong roomId)
    {
        Message? message = await _context.Messages.SingleOrDefaultAsync(m => m.Id == messageId);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ViewRoom), new { roomId });
    }

    [HttpGet]
    public async Task<IActionResult> ManageRoom(ulong roomId)
    {
        Room? room = await _context.Rooms
            .Include(r => r.Users)
            .ThenInclude(ru => ru.User)
            .SingleOrDefaultAsync(r => r.Id == roomId);

        User cu = await GetCurrentUserAsync();
        RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == cu.Id && ru.RoomId == roomId);

        if (room != null && ru != null)
        {
            RoomViewModel model = new()
            {
                Room = room,
                Perks = ru.Perks,
                CurrentUserId = cu.Id
            };

            ViewData["roomName"] = ru.Room.IsGroup ? ru.Room.Name : "Private chat";
            ViewData["roomId"] = ru.Room.Id;
            ViewData["managePage"] = true;

            return View(model);
        }

        return RedirectToAction(nameof(ViewRoom));
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRoomInfo(string roomName, ulong roomId)
    {
        if (await CheckCurrentUserPerkAsync(RoomPerks.ChangeRoomInfo, roomId))
        {
            Room? room = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId);
            if (room != null && !string.IsNullOrEmpty(roomName)) room.Name = roomName;

            await _context.SaveChangesAsync();

            // TODO: push update for rooms list of all users of this chat 
            // TODO: push update for all  users, if they were in this chat atm
            // TODO: push update for all users that were in manageRoom page atm
        }
        else
        {
            return Forbid(); // TODO: prettify
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> AddUser(string userName, ulong roomId)
    {
        User? user = await _userManager.FindByNameAsync(userName);
        Room? room = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId);

        if (user != null && room != null)
        {
            RoomUser ru = new() { UserId = user.Id, RoomId = roomId, Perks = room.NewUserPerks };

            await _context.RoomUsers.AddAsync(ru);
            await _context.SaveChangesAsync();

            // TODO: push update for rooms list of involved user 
            // TODO: push update for all users that were in manageRoom page atm
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveUser(string userId, ulong roomId)
    {
        RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);
        if (ru != null)
        {
            _context.RoomUsers.Remove(ru);
            await _context.SaveChangesAsync();

            // TODO: push update for all affected users, if they were in this chat atm
            // TODO: push update for all users that were in manageRoom page atm
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> Mute(bool mute, string userId, ulong roomId)
    {
        if (
            userId != (await GetCurrentUserAsync()).Id
            && await CheckCurrentUserPerkAsync(RoomPerks.ChangeReadWritePerk, roomId)
        )
        {
            RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);
            if (ru != null)
            {
                if (mute) ru.Perks ^= RoomPerks.Write;
                else ru.Perks |= RoomPerks.Write;

                await _context.SaveChangesAsync();

                // TODO: push update for all affected users, if they were in this chat atm
                // TODO: push update for all users that were in manageRoom page atm
            }
        }
        else
        {
            return Forbid(); // TODO: prettify
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> Deafen(bool deaf, string userId, ulong roomId)
    {
        if (
            userId != (await GetCurrentUserAsync()).Id
            && await CheckCurrentUserPerkAsync(RoomPerks.ChangeReadWritePerk, roomId)
        )
        {
            RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);
            if (ru != null)
            {
                if (deaf) ru.Perks ^= RoomPerks.Read;
                else ru.Perks |= RoomPerks.Read;
            }

            await _context.SaveChangesAsync();

            // TODO: push update for all affected users, if they were in this chat atm
            // TODO: push update for all users that were in manageRoom page atm
        }
        else
        {
            return Forbid(); // TODO: prettify
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeAnyPerk(RoomPerks perk, string userId, ulong roomId)
    {
        if (
            userId != (await GetCurrentUserAsync()).Id
            && await CheckCurrentUserPerkAsync(RoomPerks.ChangeAnyPerk, roomId)
        )
        {
            if (
                perk >= RoomPerks.ChangeAnyPerk
                && await CheckCurrentUserPerkAsync(RoomPerks.TransferCreatorPerk, roomId)
            ) return Forbid(); // TODO: prettify

            RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);
            if (ru != null) ru.Perks = perk;

            await _context.SaveChangesAsync();

            // TODO: push update for all affected users, if they were in this chat atm
            // TODO: push update for all users that were in manageRoom page atm
        }
        else
        {
            return Forbid(); // TODO: prettify
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRoomNewUsersPerks(RoomPerks perk, ulong roomId)
    {
        if (await CheckCurrentUserPerkAsync(RoomPerks.ChangeAnyPerk, roomId))
        {
            if (perk >= RoomPerks.ChangeAnyPerk) return Forbid(); // TODO: prettify

            Room? room = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId);
            if (room != null) room.NewUserPerks = perk;

            await _context.SaveChangesAsync();

            // TODO: push update for all users that were in manageRoom page atm
        }
        else
        {
            return Forbid(); // TODO: prettify
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRoom(ulong roomId)
    {
        Room? room = await _context.Rooms.SingleOrDefaultAsync(r => r.Id == roomId);
        if (room != null)
        {
            _context.Rooms.Remove(room);

            await _context.SaveChangesAsync();

            // TODO: push update for all users that were in this chat
            // TODO: force every involved user to redirect to "/?roomId=0" if they were at the moment in this chat
        }
        return RedirectToAction(nameof(ViewRoom));
    }

    [HttpPost]
    public async Task<IActionResult> TransferCreatorPerk(string userId, ulong roomId)
    {
        if (
            userId != (await GetCurrentUserAsync()).Id
            && await CheckCurrentUserPerkAsync(RoomPerks.TransferCreatorPerk, roomId)
        )
        {
            RoomUser? ru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == userId && ru.RoomId == roomId);

            User cu = await GetCurrentUserAsync();
            RoomUser? cru = await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == cu.Id && ru.RoomId == roomId);

            if (ru != null && cru != null)
            {
                ru.Perks = RoomPerks.Creator;
                cru.Perks = RoomPerks.Admin;
            }

            await _context.SaveChangesAsync();

            // TODO: push update for all users that were in manageRoom page atm
        }
        else
        {
            return Forbid(); // TODO: prettify
        }

        return RedirectToAction(nameof(ManageRoom), new { roomId });
    }

    private async Task<User> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);

    private async Task<bool> CheckCurrentUserPerkAsync(RoomPerks perk, ulong roomId)
    {
        User cu = await GetCurrentUserAsync();
        RoomPerks? cuPerks = (await _context.RoomUsers.SingleOrDefaultAsync(ru => ru.UserId == cu.Id && ru.RoomId == roomId))?.Perks;
        return cuPerks != null && (cuPerks & perk) == perk;
    }

    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
}