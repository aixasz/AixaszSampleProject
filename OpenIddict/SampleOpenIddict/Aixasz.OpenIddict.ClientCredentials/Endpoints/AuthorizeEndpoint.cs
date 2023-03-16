using Microsoft.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Aixasz.OpenIddict.ClientCredentials.Endpoints;

public static class AuthorizeEndpoint
{
    public static void AddAuthorizeEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/connect/token", async (HttpContext context, IOpenIddictApplicationManager applicationManager) =>
        {
            var request = context.GetOpenIddictServerRequest();
            if (!request.IsClientCredentialsGrantType())
            {
                throw new NotImplementedException("The specified grant is not implemented.");
            }

            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var application = await applicationManager.FindByClientIdAsync(request.ClientId) ??
                throw new InvalidOperationException("The application cannot be found.");

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

            // Use the client_id as the subject identifier.
            identity.AddClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
            identity.AddClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));
            identity.AddClaim(Claims.Scope, "api");
            identity.SetDestinations(GetDestinations);

            return Results.SignIn(
                principal: new ClaimsPrincipal(identity),
                properties: null,
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        });

        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.
        static IEnumerable<string> GetDestinations(Claim claim) => claim.Type switch
        {
            Claims.Name or Claims.Email or Claims.Role => new[] { Destinations.AccessToken, Destinations.IdentityToken },
            _ => new[] { Destinations.AccessToken }
        };
    }
}
