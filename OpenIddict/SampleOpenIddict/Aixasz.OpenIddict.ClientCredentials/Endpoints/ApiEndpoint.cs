namespace Aixasz.OpenIddict.ClientCredentials.Endpoints;

public static class ApiEndpoint
{
    public static void AddApiEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/version", () => "v1")
           .RequireAuthorization("ClientCredentialsPolicy");
    }
}
