using System.Security.Claims;
using DuendeIdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DuendeIdentityServer.Pages.Account.Register;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private UserManager<ApplicationUser> _userManager;
    
    public Index(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
    
    [BindProperty]
    public RegisterViewModel Input { get; set; }
    
    [BindProperty]
    public bool RegisterSuccess { get; set; }
    
    [BindProperty]
    public IEnumerable<IdentityError> RegisterError { get; set; }
    
    [BindProperty]
    public bool RegisterWarning { get; set; }
    
    public IActionResult OnGet(string returnUrl)
    {
        Input = new RegisterViewModel()
        {
            ReturnUrl = returnUrl,
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Input.Button != "register") return Redirect("~/");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = Input.Username,
                Email = Input.Email,
                EmailConfirmed = true
            };

            IdentityResult result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    // this adds a new claim for the user e.g. Claims can be a random value that can be anything
                    // they just have to be associated to the subject e.g. this will be returned as a jwt token
                    new Claim(JwtClaimTypes.Name, Input.FullName),
                    new Claim(JwtClaimTypes.Role, Input.Role)
                });

                RegisterSuccess = true;
            } else
            {
                RegisterWarning = true;
                RegisterError = result.Errors;

            }
        }

        return Page();
    }
}