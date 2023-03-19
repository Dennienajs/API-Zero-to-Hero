using System.Diagnostics;

namespace Movies.Api.Logging;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Incoming Request: {Method} {Path} {QueryString}", 
            context.Request.Method, 
            context.Request.Path,
            context.Request.QueryString);

        await _next(context);

        stopwatch.Stop();
        _logger.LogInformation("Outgoing Response: {StatusCode}, Elapsed Time: {ElapsedMilliseconds}ms", 
            context.Response.StatusCode, 
            stopwatch.ElapsedMilliseconds);
    }
}