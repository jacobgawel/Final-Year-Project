using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using DuendeIdentityServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;

namespace DuendeIdentityServer.Services;

public class CustomProfileService : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomProfileService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);
        
        // gets all claims associated with the user
        var existingClaims = await _userManager.GetClaimsAsync(user);
        
        // adds a claim to the Jwt token under the "username" and assigns the username to it
        context.IssuedClaims.Add(new Claim("username", user.UserName));
        context.IssuedClaims.Add(new Claim("email", user.Email));
        context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name));
        context.IssuedClaims.Add(existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Role));
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}