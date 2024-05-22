using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace DuendeIdentityServer.Pages.Diagnostics;

[SecurityHeaders]
[Authorize]
public class Index : PageModel
{
    public ViewModel View { get; set; }

    public async Task<IActionResult> OnGet()
    {
        // var localAddresses = new string[] { "::ffff:192.168.65.1", "::ffff:172.18.0.1", "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
        // if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
        // {
        //     return NotFound();
        // }

        View = new ViewModel(await HttpContext.AuthenticateAsync());

        return Page();
    }
}