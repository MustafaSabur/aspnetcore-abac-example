using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace AbacExample.Api.Authorization;

public static class SampleAuthenticationDefaults
{
    public const string AuthenticationScheme = "SampleBearer";
}

public sealed class SampleAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Sample-Subject", out var subjectValues) ||
            StringValues.IsNullOrEmpty(subjectValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var subject = subjectValues.ToString();
        var identity = new ClaimsIdentity(Scheme.Name);
        identity.AddClaim(new Claim(IdentityProviderClaims.Subject, subject));

        if (HeaderMeansMfa(Request.Headers["X-Sample-Mfa"]))
        {
            identity.AddClaim(new Claim(
                IdentityProviderClaims.AuthenticationMethod,
                IdentityProviderClaims.Mfa));
        }

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = $"{Scheme.Name} realm=\"abac-example\"";
        return Task.CompletedTask;
    }

    private static bool HeaderMeansMfa(StringValues values)
    {
        var value = values.ToString();
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(value, "mfa", StringComparison.OrdinalIgnoreCase) ||
               value == "1";
    }
}
