namespace taChat.App.Models;

[Flags]
public enum RoomPerks : ushort
{
    None = 0,
    Read = 1,
    Write = 2,
    DeleteSomeonesMessage = 4,
    ChangeReadWritePerk = 8,
    ChangeRoomInfo = 16,
    AddUser = 32,
    RemoveUser = 64,
    ChangeAnyPerk = 128, // Lower than 128
    DeleteRoom = 256, // Only creator has access
    TransferCreatorPerk = 512, // Only creator has access

    // Roles:
    Deafen = Write,
    Muted = Read,
    Default = Deafen | Muted,
    Moderator = Default | ChangeReadWritePerk | AddUser | RemoveUser | DeleteSomeonesMessage,
    Admin = Moderator | ChangeRoomInfo | ChangeAnyPerk,
    Creator = Admin | DeleteRoom | TransferCreatorPerk,
    PrivateChat = Default | DeleteRoom
}