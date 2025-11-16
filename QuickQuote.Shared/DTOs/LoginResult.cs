namespace QuickQuote.Shared.DTOs;

public record LoginResult(string Token, DateTime Expires)
{
    public string Token { get; init; } = Token;
    public DateTime ExpiresAt { get; init; } = Expires;
}