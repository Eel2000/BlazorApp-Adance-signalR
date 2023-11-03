using System.Collections.ObjectModel;
using BlazorApp_Adance_signalR.Data;

namespace BlazorApp_Adance_signalR.States;

public sealed class SessionAppState
{
    public ObservableCollection<Conversation> Conversations { get; set; } = new();
    public ObservableCollection<Message> Messages { get; set; } = new();
}