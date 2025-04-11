using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add the custom error-handling middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add the custom authentication middleware
app.UseMiddleware<AuthenticationMiddleware>();

// Add the custom logging middleware
app.UseMiddleware<LoggingMiddleware>();

var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
    new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
};

// Minimal API endpoints
app.MapGet("/users", () => users);

app.MapGet("/users/{id}", (int id) => users.FirstOrDefault(u => u.Id == id));

app.MapPost("/users", (User user) => { 
    users.Add(user); 
    return Results.Created($"/users/{user.Id}", user); 
});

app.MapPut("/users/{id}", (int id, User updatedUser) => 
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();
    
    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    return Results.Ok(user);
});

app.MapDelete("/users/{id}", (int id) => 
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user == null) return Results.NotFound();
    
    users.Remove(user);
    return Results.NoContent();
});

// Add a token generation endpoint
app.MapPost("/generate-token", (string username) =>
{
    // Use a secure and sufficiently long secret key (at least 32 characters)
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-very-secure-and-long-secret-key-12345"));
    var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

    // Create claims (you can add more claims as needed)
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    // Create the token
    var token = new JwtSecurityToken(
        issuer: "your-app",
        audience: "your-app",
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1), // Token expiration time
        signingCredentials: signingCredentials
    );

    // Return the token
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = tokenString });
});

app.Run();
// User model
public class User
{
    public int Id { get; set; }
    required public string Name { get; set; }
    required public string Email { get; set; }
}
