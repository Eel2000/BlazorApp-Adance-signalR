using BlazorApp_Adance_signalR.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp_Adance_signalR.Hubs;

[Authorize]
public class ChatHub : Hub<IChatHub>
{
    public async ValueTask MessageThem(Message message)
    {
        var user = Context.User;

        await Clients.User(message.UserId).Notification(message);
        // Console.WriteLine(message);
    }

    public async ValueTask ToAll(Message message)
        => await Clients.Others.ForAll(message);
}