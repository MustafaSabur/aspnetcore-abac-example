namespace AbacExample.Authorization;

public interface IAuthorizationProfileLoader
{
    Task<AuthorizationProfile?> LoadBySubjectAsync(
        string subject,
        CancellationToken cancellationToken = default);
}