namespace UserManagementAPI.Middleware
{
    /// <summary>
    /// Middleware that enforces API-key authentication on all non-Swagger routes.
    /// 
    /// The client must include the header:  X-Api-Key: your-secret-key
    /// 
    /// In a production system the allowed keys would come from a secrets store
    /// (e.g., Azure Key Vault, AWS Secrets Manager, or environment variables).
    /// Here we read from appsettings.json for simplicity.
    /// 
    /// Registered in Program.cs with: app.UseMiddleware&lt;ApiKeyAuthenticationMiddleware&gt;();
    /// </summary>
    public class ApiKeyAuthenticationMiddleware
    {
        private const string ApiKeyHeader = "X-Api-Key";

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip authentication for Swagger UI and OpenAPI spec endpoints
            var path = context.Request.Path.Value ?? string.Empty;
            if (IsSwaggerPath(path))
            {
                await _next(context);
                return;
            }

            // Check for the API key header
            if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var suppliedKey))
            {
                _logger.LogWarning("Request rejected — missing {Header} header. Path: {Path}", ApiKeyHeader, path);
                context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """{"success":false,"message":"API key is missing. Include the X-Api-Key header."}""");
                return;
            }

            // Validate against configured key(s)
            var validKey = _configuration["ApiKey"] ?? "dev-secret-key-12345";
            if (!suppliedKey.Equals(validKey))
            {
                _logger.LogWarning("Request rejected — invalid API key. Path: {Path}", path);
                context.Response.StatusCode  = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    """{"success":false,"message":"Invalid API key."}""");
                return;
            }

            await _next(context);
        }

        private static bool IsSwaggerPath(string path) =>
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
            path == "/";
    }
}
