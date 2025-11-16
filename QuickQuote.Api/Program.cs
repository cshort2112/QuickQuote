using QuickQuote.Shared.DTOs;

var builder = WebApplication.CreateBuilder(args);

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

// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }



app.MapPost("/api/auth/login", async (LoginRequest request) =>
{
    string username = request.Username;
    string password = request.Password;

    if (username == "admin" && password == "123987")
    {
        string token = "token";
        return Results.Ok(new LoginResult(token, DateTime.Now.AddHours(1)));
    }
    return Results.Unauthorized();
});

app.Run();

record LoginRequest(string Username, string Password);