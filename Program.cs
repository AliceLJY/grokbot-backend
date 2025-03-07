using GrokBot.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// 添加控制器
builder.Services.AddControllers();

// 允许更大的JSON请求体大小
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// 增加请求体大小限制
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});

// 配置CORS
builder.Services.AddCors(options =>
{
    // 默认CORS策略
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    
    // 更宽松的CORS策略，用于需要特殊处理的端点
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Content-Disposition", "Content-Length");
    });
});

// 添加日志
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 添加HttpClient服务
builder.Services.AddHttpClient();
builder.Services.AddSingleton<GrokService>();

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
            
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;
            
            var result = JsonSerializer.Serialize(new 
            { 
                error = "Internal Server Error",
                message = exception?.Message,
                path = context.Request.Path
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

// 重要：正确的中间件顺序
// 首先应用路由
app.UseRouting();

// 然后应用CORS - 必须在 UseRouting 和 MapControllers 之间
app.UseCors();

// 添加健康检查终结点
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// 映射控制器 (相当于 UseEndpoints)
app.MapControllers();

// 输出环境信息
var environmentName = app.Environment.EnvironmentName;
var logger = app.Services.GetService<ILogger<Program>>();
logger?.LogInformation("Application starting in {Environment} environment", environmentName);
logger?.LogInformation("GrokAPI Key is {Present}", !string.IsNullOrEmpty(builder.Configuration["GrokApi:ApiKey"]) ? "present" : "missing");

// 运行应用
app.Run();
