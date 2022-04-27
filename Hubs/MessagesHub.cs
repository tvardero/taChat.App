using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;

namespace taChat.App.Hubs;

public class MessagesHub : Hub
{
    public string GetConnectionId() => Context.ConnectionId;

    //addtogroupasync here
}