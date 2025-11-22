namespace QuickQuote.Shared.DTOs;

public class QuickQuoteLoginSecret
{
    public string Username { get; init; } = default!;
    public string PasswordHash { get; init; } = default!;
    public string Salt { get; init; } = default!;
    public int Iterations { get; init; }
    
}