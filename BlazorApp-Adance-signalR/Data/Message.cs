using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp_Adance_signalR.Data;

#nullable disable

public class Message
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    /// <summary>
    /// The destination user
    /// </summary>
    public string UserId { get; set; }

    [Required(ErrorMessage = "the send user is mandatory")]
    public string SenderId { get; set; }

    public string Content { get; set; }

    public DateTime SentAt { get; set; }

    /// <summary>
    /// specifies if the message has been seen and read bu the user(receiver)
    /// </summary>
    public bool Seen { get; set; }

    public Guid ConversationId { get; set; }

    public Conversation Conversation { get; set; }
}