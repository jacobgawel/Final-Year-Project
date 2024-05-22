using Duende.IdentityServer.Models;

namespace DuendeIdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("parkingApp", "Full access to the app"),

        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman",
                
                // this correlates to the resources that are defined on line 8
                // we will be returning 2 tokens essentially to access them 
                // Access token and Id token (id tokens contains information about the user)
                AllowedScopes = {"openid", "profile", "parkingApp"},
                RedirectUris = {"https://www.getpostman.com/oauth2/callback"},
                // this is the client token that we will be sending over here to request the token
                ClientSecrets = new [] {new Secret("NotASecret".Sha256())},
                // this specified that you can login using the resource owner password e.g. for bob it would be Pass123$
                AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
            },
            new Client
            {
                ClientId = "nextApp",
                ClientName = "nextApp",
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RequirePkce = false,
                AllowedScopes = {"openid", "profile", "parkingApp"},
                AllowOfflineAccess = true,
                AccessTokenLifetime = 3600*24*30,
                RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
                ClientSecrets = new [] {new Secret("NotASecret".Sha256())},
                AlwaysIncludeUserClaimsInIdToken = true,
            }
        };
}