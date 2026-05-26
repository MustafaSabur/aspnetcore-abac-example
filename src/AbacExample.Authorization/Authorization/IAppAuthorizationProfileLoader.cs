namespace AbacExample.Authorization;

public interface IAppAuthorizationProfileLoader
{
    Task<AppAuthorizationProfile?> LoadBySubjectAsync(
        string subject,
        CancellationToken cancellationToken = default);
}