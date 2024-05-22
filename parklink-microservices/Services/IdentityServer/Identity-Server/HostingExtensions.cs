using Duende.IdentityServer;
using DuendeIdentityServer.Data;
using DuendeIdentityServer.Models;
using DuendeIdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DuendeIdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireUppercase = false;
            // Added this to make testing and password input faster. In production this would be likely "true" to make passwords more secure.
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                
                // This fixes the 401 error that occurs when postman attempts to
                // authenticate. This is because the localhost in the resource (authority) does not match
                // the issuerUrl when this is not set. By default its set as "localhost:8101"
                if (builder.Environment.IsEnvironment("Docker"))
                {
                    options.IssuerUri = "identity-server";
                }

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                // removed the line of code below because we dont need at this stage and it will cause problems
                // options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            // lets us add additional content to the jwt
            .AddProfileService<CustomProfileService>();
        
        // adds identity server options/preferences for user identities
        builder.Services.Configure<IdentityOptions>(options =>
        {
            // this option makes sure that the users saved within the db are unique
            options.User.RequireUniqueEmail = true;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

        builder.Services.AddAuthentication();

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}