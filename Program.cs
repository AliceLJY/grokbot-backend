using GrokBot.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HttpClient for GrokService
builder.Services.AddHttpClient<GrokService>();
builder.Services.AddScoped<GrokService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGitHubPages", policy =>
    {
        policy.WithOrigins(
                "https://aliceljy.github.io",
                "https://aliceljy.github.io/grokbot")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173", 
                "http://localhost:5000", 
                "http://localhost:5500",
                "https://localhost:5173",
                "https://localhost:5000", 
                "https://localhost:5500")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowLocalhost");
}
else
{
    // 在生产环境中使用更宽松的CORS策略以确保跨域请求能够成功
    app.UseCors("AllowAll");
}

// 禁用HTTPS重定向，避免混合内容问题
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// 打印环境信息，帮助调试
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"ContentRoot: {app.Environment.ContentRootPath}");
Console.WriteLine($"WebRootPath: {builder.Environment.WebRootPath ?? "Not Set"}");
Console.WriteLine($"ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "Not Set"}");
Console.WriteLine($"GrokApi__ApiKey: {(string.IsNullOrEmpty(builder.Configuration["GrokApi:ApiKey"]) ? "Not Set" : "Is Set")}");

app.Run();
