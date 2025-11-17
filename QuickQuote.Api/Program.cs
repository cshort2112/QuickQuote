using QuickQuote.Api.Services;
using QuickQuote.Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<IVaultService, VaultService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
app.UseHttpsRedirection();

//will be replacing this with a key vault, and a true JWT.. for now we are just going to go with the key vault.
//I installed hashicorp keyvault on my local machine, and added the secrets to it.
//I also created a plist launch daemon to run the key vault on startup.
//ever time you restart the machine however you must run these commands to unseal the vault:
//export VAULT_ADDR='http://127.0.0.1:8200'
//vault operator unseal # key 1
//vault operator unseal # key 2
//vault operator unseal # key 3
app.MapPost("/api/auth/login", async (LoginRequest request, IVaultService vaultService) =>
{
    if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.Unauthorized();
    }
    var loginSecret = await vaultService.GetLoginSecretAsync(request.Username);
    if (loginSecret == null)
    {
        return Results.Unauthorized();
    }
    
    string passwordHash = loginSecret.PasswordHash;
    string salt = loginSecret.Salt;
    string iterations = loginSecret.Iterations.ToString();
    string username = request.Username;
    string password = request.Password;


    if (PasswordVerifier.VerifyPassword(password, passwordHash, salt, int.Parse(iterations)))
    {
        string token = "token";
        return Results.Ok(new LoginResult(token, DateTime.Now.AddDays(1)));
    }
    //default
    return Results.Unauthorized();
});

app.Run();

record LoginRequest(string? Username, string? Password);