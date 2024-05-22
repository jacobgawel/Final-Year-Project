using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using IdentityModel;
using DuendeIdentityServer.Data;
using DuendeIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DuendeIdentityServer;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
        context.Database.Migrate();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (userMgr.Users.Any()) return;


        // TODO: Add more users when shit is figured out e.g. provider account and attendant account
        
        var bob = userMgr.FindByNameAsync("bob").Result;
        if (bob == null)
        {
            bob = new ApplicationUser
            {
                Id = "68b544ff-0f41-48af-af50-20344493d682",
                UserName = "bob",
                Email = "bob@gmail.com",
                EmailConfirmed = true,
            };
            var result = userMgr.CreateAsync(bob, "pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(bob, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                new Claim(JwtClaimTypes.Role, "admin")
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("bob created");
        }
        else
        {
            Log.Debug("bob already exists");
        }

        var jakub = userMgr.FindByNameAsync("jakubgawel").Result;
        if (jakub == null)
        {
            jakub = new ApplicationUser
            {
                Id = "e8c4d183-ff45-43a8-882d-563ceeacf5eb",
                UserName = "jakubgawel",
                Email = "jakubgawel@icloud.com",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(jakub, "pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(jakub, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Jakub Gawel"),
                new Claim(JwtClaimTypes.Role, "user")
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("jakubgawel created");
        }
        else
        {
            Log.Debug("jakubgawel already exists");
        }
        
        var jakeyboi = userMgr.FindByNameAsync("jakeyboi").Result;
        if (jakeyboi == null)
        {
            jakeyboi = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "jakeyboi",
                Email = "jakegawel1310@outlook.com",
                EmailConfirmed = true
            };
            var result = userMgr.CreateAsync(jakeyboi, "pass123$").Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(jakeyboi, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, "Jakub Gawel"),
                new Claim(JwtClaimTypes.Role, "provider")
            }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug("jakeyboi created");
        }
        else
        {
            Log.Debug("jakeyboi already exists");
        }
    }
}