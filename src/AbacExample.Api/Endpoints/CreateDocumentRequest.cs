namespace AbacExample.Api.Endpoints;

public sealed record CreateDocumentRequest(bool IsConfidential, string? Summary);
