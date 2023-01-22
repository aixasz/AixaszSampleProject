using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server.AspNetCore;
using SampleOpenIddict.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Configure Entity Framework Core to use Microsoft SQL Server.
    options.UseSqlServer(connectionString);

    // Register the entity sets needed by OpenIddict.
    // Note: use the generic overload if you need to replace the default OpenIddict entities.
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()

        // Register the OpenIddict core components.
        .AddCore(options =>
        {
            // Configure OpenIddict to use the Entity Framework Core stores and models.
            // Note: call ReplaceDefaultEntities() to replace the default entities.
            options.UseEntityFrameworkCore()
                   .UseDbContext<ApplicationDbContext>();
        })

        // Register the OpenIddict server components.
        .AddServer(options =>
        {
            // Enable the token endpoint.
            options.SetTokenEndpointUris("connect/token");

            // Enable the client credentials flow.
            options.AllowClientCredentialsFlow();

            // Register the signing and encryption credentials.
            options.AddDevelopmentEncryptionCertificate()
                   .AddDevelopmentSigningCertificate();

            // Register the ASP.NET Core host and configure the ASP.NET Core options.
            options.UseAspNetCore()
                   .EnableTokenEndpointPassthrough();
        })

        // Register the OpenIddict validation components.
        .AddValidation(options =>
        {
            // Import the configuration from the local OpenIddict server instance.
            options.UseLocalServer();

            // Register the ASP.NET Core host.
            options.UseAspNetCore();
        });

builder.Services.AddOpenIddict();

var app = builder.Build();

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
    identity.SetDestinations(GetDestinations);

    return Results.SignIn(
        principal: new ClaimsPrincipal(identity),
        properties: null,
        authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
});

static IEnumerable<string> GetDestinations(Claim claim)
{
    // Note: by default, claims are NOT automatically included in the access and identity tokens.
    // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
    // whether they should be included in access tokens, in identity tokens or in both.
    return claim.Type switch
    {
        Claims.Name or Claims.Email or Claims.Role => new[] { Destinations.AccessToken, Destinations.IdentityToken },
        _ => new[] { Destinations.AccessToken }
    };
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();
