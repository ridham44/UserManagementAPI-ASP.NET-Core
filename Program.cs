using UserManagementAPI.Data;
using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "User Management API", 
        Version = "v1",
        Description = "A RESTful API for managing users with CRUD operations, validation, and middleware."
    });
});

// Register the in-memory user repository as a singleton
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline

// Custom logging middleware (runs for every request)
app.UseMiddleware<RequestLoggingMiddleware>();

// Custom authentication middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
