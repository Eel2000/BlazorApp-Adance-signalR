using BlazorApp_Adance_signalR.Data;

namespace BlazorApp_Adance_signalR.Hubs
{
    public interface IChatHub
    {
        Task Notification(Message message);
    }
}
