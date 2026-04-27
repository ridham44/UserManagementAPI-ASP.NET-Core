using System.Diagnostics;

namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware that logs every incoming HTTP request and outgoing response.
    /// Captured details: method, path, status code, elapsed time, and client IP.
    /// 
    /// Registered in Program.cs with: app.UseMiddleware&lt;RequestLoggingMiddleware&gt;();
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            var method     = context.Request.Method;
            var path       = context.Request.Path;
            var query      = context.Request.QueryString;
            var clientIp   = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var requestId  = Guid.NewGuid().ToString("N")[..8]; // Short 8-char trace ID

            _logger.LogInformation(
                "[{RequestId}] → {Method} {Path}{Query} | IP: {ClientIp}",
                requestId, method, path, query, clientIp);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var statusCode = context.Response.StatusCode;
                var elapsed    = stopwatch.ElapsedMilliseconds;

                var level = statusCode >= 500
                    ? LogLevel.Error
                    : statusCode >= 400
                        ? LogLevel.Warning
                        : LogLevel.Information;

                _logger.Log(level,
                    "[{RequestId}] ← {StatusCode} | {Method} {Path} | {Elapsed}ms",
                    requestId, statusCode, method, path, elapsed);
            }
        }
    }
}
