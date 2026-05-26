namespace AbacExample.Api.Endpoints;

public sealed record CreateCaseFileRequest(bool IsConfidential, string? Summary);
