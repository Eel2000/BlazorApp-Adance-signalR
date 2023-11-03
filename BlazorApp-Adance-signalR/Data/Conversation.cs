using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp_Adance_signalR.Data;

public sealed class Conversation
{
    public Conversation()
    {
        Messages = new HashSet<Message>();
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public string? StarterId { get; set; }

    public string? ReceiverId { get; set; }

    public long MessagesCount => Messages.LongCount();
    public ICollection<Message> Messages { get; set; }
}