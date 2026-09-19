namespace Backend.Application.Abstractions;

/// <summary>Token issued at sign-in. Real JWT minting lives in Infrastructure.</summary>
public sealed record AuthToken(string Token, DateTimeOffset ExpiresAt);
