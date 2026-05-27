using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AbacExample.Authorization;

public static class AbacAuthorizationServiceCollectionExtensions
{
    public static IServiceCollection AddAbacAuthorizationCore(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, CurrentUser>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, PermissionAuthorizationHandler>());
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }

    public static IServiceCollection AddAuthorizationProfileEnrichment(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IClaimsTransformation, AuthorizationProfileClaimsTransformation>());

        return services;
    }
}
