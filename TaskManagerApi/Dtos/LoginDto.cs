using System.ComponentModel.DataAnnotations;

namespace TaskManagerApi.Dtos;

// اللي بيبعته الـ client لما يعمل login
public class LoginDto
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = "";
}