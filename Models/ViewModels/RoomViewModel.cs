namespace taChat.App.Models.ViewModels;

public class RoomViewModel
{
    public Room? Room { get; init; } = null!;

    public string CurrentUserId { get; init; } = null!;
}