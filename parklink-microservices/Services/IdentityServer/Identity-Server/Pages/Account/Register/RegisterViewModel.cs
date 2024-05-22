using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DuendeIdentityServer.Pages.Account.Register;

public class RegisterViewModel
{
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string FullName { get; set; }
    [Required]
    public string Role { get; set; }
    public List<SelectListItem> Roles { get; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "admin", Text = "Admin" },
        new SelectListItem { Value = "provider", Text = "Provider" },
        new SelectListItem { Value = "user", Text = "User"  },
    };
    public string ReturnUrl { get; set; }
    public string Button { get; set; }
}