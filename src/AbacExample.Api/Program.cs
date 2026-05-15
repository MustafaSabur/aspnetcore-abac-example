using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var useSampleAuthentication = builder.Configuration.GetValue(
    "SampleAuthentication:Enabled",
    builder.Environment.IsDevelopment());

var defaultScheme = useSampleAuthentication
    ? SampleAuthenticationDefaults.AuthenticationScheme
    : JwtBearerDefaults.AuthenticationScheme;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = defaultScheme;
        options.DefaultChallengeScheme = defaultScheme;
    })
    .AddScheme<AuthenticationSchemeOptions, SampleAuthenticationHandler>(
        SampleAuthenticationDefaults.AuthenticationScheme,
        options => { })
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];

        // Keep OpenID Connect claim names like "sub" and "amr".
        options.MapInboundClaims = false;
    });

if (!useSampleAuthentication &&
    (string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Authority"]) ||
     string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Audience"])))
{
    throw new InvalidOperationException(
        "Configure Authentication:Authority and Authentication:Audience, or enable SampleAuthentication for local demos.");
}

var appUserPolicy = new AuthorizationPolicyBuilder(defaultScheme)
    .RequireAuthenticatedUser()
    .RequireClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True)
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(appUserPolicy)
    .SetFallbackPolicy(appUserPolicy);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IClaimsTransformation, AppClaimsTransformation>();
builder.Services.AddSingleton<IAppAuthorizationProfileLoader, InMemoryAppAuthorizationProfileLoader>();
builder.Services.AddSingleton<IAppointmentRepository, InMemoryAppointmentRepository>();
builder.Services.AddSingleton<IAuthorizationAuditSink, LoggingAuthorizationAuditSink>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuthorizationHandler, AppointmentAbacHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapSampleEndpoints(useSampleAuthentication);
app.MapAppointmentEndpoints();

app.Run();
