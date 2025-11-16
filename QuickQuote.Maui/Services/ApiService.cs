using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using QuickQuote.Shared.DTOs;


namespace QuickQuote.Maui.Services;

public class ApiService
{
    private static readonly HttpClient client = new();

    public static async Task<LoginResult?> LoginAsync(string? username, string? password)
    {
        var baseUrl = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5030" : "http://localhost:5030";
        var loginData = new { username = username, password = password };
        var json = JsonSerializer.Serialize(loginData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            var response = await client.PostAsync("http://10.0.2.2:5030/api/auth/login", content);
            
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