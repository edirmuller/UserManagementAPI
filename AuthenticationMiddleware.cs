using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Bypass authentication for the /generate-token endpoint
        if (context.Request.Path.StartsWithSegments("/generate-token"))
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No token provided in the request.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: No token provided.");
            return;
        }

        try
        {
            // Validate the token (basic validation for demonstration purposes)
            var jwtHandler = new JwtSecurityTokenHandler();
            if (!jwtHandler.CanReadToken(token))
            {
                _logger.LogWarning("Invalid token format.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid token.");
                return;
            }

            var jwtToken = jwtHandler.ReadJwtToken(token);

            // Additional validation logic can be added here (e.g., checking claims, expiration, etc.)
            _logger.LogInformation("Token is valid. Proceeding to the next middleware.");
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Token validation failed.");
        }
    }
}