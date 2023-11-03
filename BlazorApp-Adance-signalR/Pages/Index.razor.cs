using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using BlazorApp_Adance_signalR.Data;
using BlazorApp_Adance_signalR.States;
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

    [Inject] public SessionAppState? State { get; set; }

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

        foreach (var conversation in _conversations)
            State?.Conversations.Add(conversation);

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

                State?.Messages.Add(msg);

                InvokeAsync(StateHasChanged);
            });

            hubConnection?.On<Message>("ForAll", (msg) =>
            {
                _messages.Add(msg);

                State?.Messages.Add(msg);

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

        var conversations =
            _conversations.FirstOrDefault(c => c.StarterId == authenticationState?.User?.Identity?.Name
                                               && c.ReceiverId == selectedUser.Email);

        if (conversations is not null)
            _messages = new(conversations?.Messages);
        else
        {
            _messages = new();
        }

        StateHasChanged();
    }

    async Task SendMessage()
    {
        try
        {
            // var authenticateState = await authenticationStateTask!;

            var conversation = await StartNewConversationIfNotExist(selectedUser.Email);

            Message msg = new()
            {
                UserId = selectedUser?.Email,
                Content = message,
                SenderId = authenticationState?.User?.Identity?.Name,
                SentAt = DateTime.Now,
                ConversationId = conversation.Id,
                Id = Guid.NewGuid()
            };

            if (selectedUser is null)
            {
                await hubConnection?.SendAsync("ToAll", msg)!;
            }
            else
            {
                await hubConnection?.SendAsync("MessageThem", msg)!;
                conversation.Messages.Add(msg);
            }

            this.message = string.Empty;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    async Task<Conversation?> StartNewConversationIfNotExist(string receiver)
    {
        try
        {
            var username = authenticationState?.User?.Identity?.Name;

            var existingConversation = _conversations.FirstOrDefault(c =>
                c.StarterId == username || c.ReceiverId == username);

            if (existingConversation is not null)
                return existingConversation;

            var newConvesation = new Conversation
            {
                StarterId = username,
                ReceiverId = receiver,
            };

            var entry = await Context.Conversations.AddAsync(newConvesation);
            await Context.SaveChangesAsync();
            return entry.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return default;
        }
    }


    public void Dispose()
    {
        Console.Write("Page's quite or reloaded\r\n");
    }
}