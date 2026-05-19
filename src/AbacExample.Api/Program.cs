using AbacExample.Api.Authorization;
using AbacExample.Api.Data;
using AbacExample.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
    .SetFallbackPolicy(appUserPolicy)
    .AddAppPermissionPolicies();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddTransient<IClaimsTransformation, AppClaimsTransformation>();
builder.Services.AddScoped<IAppAuthorizationProfileLoader, DbAppAuthorizationProfileLoader>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DocumentAbacHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AbacExample"));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapDocumentEndpoints();

app.Run();
