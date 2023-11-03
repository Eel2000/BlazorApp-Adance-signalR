using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using BlazorApp_Adance_signalR.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp_Adance_signalR.Pages;

public partial class Index : ComponentBase, IDisposable
{
    [CascadingParameter] private Task<AuthenticationState>? authenticationStateTask { get; set; }

    [Inject] public AuthenticationDbContext? Context { get; set; }

    HubConnection? hubConnection;
    private AuthenticationState? authenticationState;

    string? message = string.Empty;
    List<IdentityUser> _users = new();
    IdentityUser? selectedUser = default;
    ObservableCollection<Message> _messages = new();
    private ObservableCollection<Conversation> _conversations = new();


    protected override async Task OnInitializedAsync()
    {
        authenticationState = await authenticationStateTask!;
        var users = userManager.Users.Where(x => x.Email != authenticationState.User.Identity.Name);
        _users = new(users);


        _conversations.CollectionChanged += ConversationsOnCollectionChanged;
        _messages.CollectionChanged += MessagesOnCollectionChanged;

        await BuildConnectionAsync();
    }

    private void MessagesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _messages = new(_messages);
        InvokeAsync(StateHasChanged);
    }

    private void ConversationsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _conversations = new(_conversations);
        InvokeAsync(StateHasChanged);
    }


    async ValueTask BuildConnectionAsync()
    {
        try
        {
            var cookieContainer = new CookieContainer();
            var url = Navigation.ToAbsoluteUri("/chat");
            var cookie = new Cookie()
            {
                Name = ".aspnetcore.identity.application",
                Domain = url.Host,
                Value = CookieProvider.Cookie
            };
            cookieContainer.Add(cookie);

            hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation.ToAbsoluteUri("/chat"), options =>
                {
                    options.Cookies = cookieContainer;

                    options.HttpMessageHandlerFactory = (msg) =>
                    {
                        if (msg is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback +=
                                (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        }

                        return msg;
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            InitializeMessaging();

            await hubConnection!.StartAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    async ValueTask InitializeMessaging()
    {
        try
        {
            hubConnection?.On<Message>("Notification", (msg) =>
            {
                _messages.Add(msg);
                _messages = new(_messages.OrderBy(x => x.SentAt));
                InvokeAsync(StateHasChanged);
            });

            hubConnection?.On<Message>("ForAll", (msg) =>
            {
                _messages.Add(msg);
                InvokeAsync(StateHasChanged);
            });


            var oldConversations = await Context?.Conversations.Where(x =>
                    x.StarterId == authenticationState.User.Identity.Name ||
                    x.ReceiverId == authenticationState.User.Identity.Name)
                .Include(c => c.Messages)
                .ToListAsync();

            _conversations = new(oldConversations);

            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    void SelectUser(IdentityUser user)
    {
        selectedUser = user;
        StateHasChanged();
    }

    async Task SendMessage()
    {
        // var authenticateState = await authenticationStateTask!;


        Message? message = new()
        {
            UserId = selectedUser?.Email,
            Content = this.message,
            SenderId = authenticationState?.User?.Identity?.Name,
            SentAt = DateTime.Now,
            Id = Guid.NewGuid()
        };

        if (selectedUser is null)
        {
            await hubConnection?.SendAsync("ToAll", message);
        }
        else
        {
            await hubConnection?.SendAsync("MessageThem", message);
        }

        this.message = string.Empty;
        await InvokeAsync(StateHasChanged);
    }


    public void Dispose()
    {
        Console.Write("Page's quite or reloaded\r\n");
    }
}