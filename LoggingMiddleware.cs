using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the HTTP request
        var request = context.Request;
        Debug.WriteLine($"Request: {request.Method} {request.Path}");

        // Call the next middleware in the pipeline
        await _next(context);

        // Log the HTTP response
        var response = context.Response;
        Debug.WriteLine($"Response: {response.StatusCode}");
    }
}