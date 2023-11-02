using System.ComponentModel.DataAnnotations;

namespace BlazorApp_Adance_signalR.Areas.Identity.Data;

#nullable disable

public class InputModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }


    [Required]
    public string Password { get; set; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = "The password and the confirm-password does not match")]
    public string ConfirmPassword { get; set; }
}
