using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Mango.Service.Identity
{
    public class SD
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        public static IEnumerable<IdentityResource> IdentityResources => new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Email(),
            new IdentityResources.Profile()
        };

        public static IEnumerable<ApiScope> ApiScopes => new List<ApiScope>
        {
            new ApiScope("mango", "Mango Server"),
            new ApiScope(name: "read",  displayName: "Read the data"),
            new ApiScope(name: "write",  displayName: "Write the data"),
            new ApiScope(name: "delete", displayName: "Delete the data"),
        };

        public static IEnumerable<Client> Clients => new List<Client>
        {
            new Client{ 
                ClientId = "client", 
                ClientSecrets = { new Secret("secret".Sha256()) }, 
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes  = { "read", "write","profile"}
            },
            new Client{
                ClientId = "mango",
                ClientSecrets = { new Secret("mango".Sha256()) },
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = {"https://localhost:7242/signin-oidc" },
                PostLogoutRedirectUris = {"https://localhost:7242/signout-callback-oidc" },
                AllowedScopes  = new List<string>{
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email,
                    "mango"
                }
            }
        };
    }
}
