using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];

        // Keep OpenID Connect claim names like "sub" and "amr".
        options.MapInboundClaims = false;
    });

var appUserPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser()
    .RequireClaim(AppClaims.ProfileLoaded, BooleanClaimValues.True)
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(appUserPolicy)
    .SetFallbackPolicy(appUserPolicy);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IClaimsTransformation, AppClaimsTransformation>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuthorizationHandler, AppointmentAbacHandler>();

// Add project-specific infrastructure in the future application:
// builder.Services.AddScoped<IAppAuthorizationProfileLoader, DbAppAuthorizationProfileLoader>();
// builder.Services.AddDbContext<AppDbContext>(options => { /* configure database provider */ });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAppointmentEndpoints();

app.Run();
