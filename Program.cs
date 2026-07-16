using GrokBot.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 64 * 1024;
});

var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Restricted", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 添加日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// 在生产环境中使用异常处理
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var result = JsonSerializer.Serialize(new 
            { 
                error = "Internal Server Error"
            });
            
            await context.Response.WriteAsync(result);
        });
    });
}
else
{
    // 开发环境中显示详细错误
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors("Restricted");

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Archived",
    ChatEnabled = false,
    Timestamp = DateTime.UtcNow
}));

app.MapControllers();

var environmentName = app.Environment.EnvironmentName;
var logger = app.Services.GetService<ILogger<Program>>();
logger?.LogInformation("Application starting in {Environment} environment", environmentName);
logger?.LogWarning("Archived service mode: chat proxy is disabled");

app.Run();
