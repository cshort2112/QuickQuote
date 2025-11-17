using System.Text.Json;
using QuickQuote.Shared.DTOs;

namespace QuickQuote.Api.Services;

public class VaultService : IVaultService
{
    private readonly HttpClient _httpClient;
    private readonly string _vaultAddress;
    private readonly string _vaultToken;

    public VaultService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _vaultAddress = config["Vault:Address"]
            ?? throw new InvalidOperationException("Vault:Address not found in configuration");
        //this gets pulled from an env variable.
        _vaultToken = config["Vault:Token"]
            ?? throw new InvalidOperationException("Vault:Token not found in configuration");
    }

    public async Task<QuickQuoteLoginSecret?> GetLoginSecretAsync(
        string username, CancellationToken cancellationToken = default)
    {
        //normalized username to lowercase
        var userKey = username.ToLowerInvariant();
        
        //KV v2 read path: /v1/<mount>/data/<path>
        var url = $"{_vaultAddress}/v1/secret/data/quickquote-logins/{userKey}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Vault-Token", _vaultToken);
        
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // 404 -> user not found
            return null;
        }
        
        using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        //KV v2 response structure
        //{
        // "data": {
        //  "data": {
        //   "username": "...",
        //   "passwordHash": "...",
        //   "salt": "...",
        //   "iterations": 100000
        //  },
        //  "metadata": { ... }
        // }
        //}
        using var doc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);

        var dataRoot = doc.RootElement.GetProperty("data").GetProperty("data");

        var secret = new QuickQuoteLoginSecret
        {
            Username = dataRoot.GetProperty("username").GetString() ?? "",
            PasswordHash = dataRoot.GetProperty("passwordHash").GetString() ?? "",
            Salt = dataRoot.GetProperty("salt").GetString() ?? "",
            Iterations = dataRoot.GetProperty("iterations").GetInt32()
        };
        
        return secret;
    }
    
    
}