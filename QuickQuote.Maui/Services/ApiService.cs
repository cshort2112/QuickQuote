using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Android.OS;
using QuickQuote.Shared.DTOs;
using Debug = System.Diagnostics.Debug;


namespace QuickQuote.Maui.Services;

public class ApiService
{
    //adding below to allow self signed certs on android.
#if (DEBUG)
    private static readonly HttpClientHandler _handler = new()
        { ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true };

    private static readonly HttpClient client = new(_handler);
#else
    private static readonly HttpClient client = new();
#endif


    public static async Task<LoginResult?> LoginAsync(string? username, string? password)
    {
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7265"
            : "https://localhost:7265";
        var loginData = new { username = username, password = password };
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await client.PostAsync(baseUrl + "/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResult>();
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error, Initiating Security Protocol 13255: " + e);
            throw;
        }
    }
}