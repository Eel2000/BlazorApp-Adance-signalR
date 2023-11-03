using System.Collections.ObjectModel;

namespace BlazorApp_Adance_signalR.States;

public sealed class GlobalAppState
{
    public List<string> ActiveUser { get; set; } = new();
}