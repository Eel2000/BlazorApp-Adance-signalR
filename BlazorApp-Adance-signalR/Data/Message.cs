using System.ComponentModel.DataAnnotations;

namespace BlazorApp_Adance_signalR.Data;

#nullable disable

public class Message
{
    [Key]
    public Guid Id { get; set; }

    public string UserId { get; set; }
    public string SenderId { get; set; }
    public string Content { get; set; }

    public DateTime SentAt { get; set; }
}
