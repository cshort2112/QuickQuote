using QuickQuote.Shared.DTOs;

namespace QuickQuote.Api.Services;

public interface IVaultService
{
    //I just learned about cancellation tokens, so this is a good time to use them.
    Task<QuickQuoteLoginSecret?> GetLoginSecretAsync(string username, CancellationToken cancellationToken = default);
}