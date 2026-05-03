using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Manabi.Api.Commands;
using Manabi.Api.Data;
using Manabi.Api.Hubs;
using Manabi.Api.Models;
using Manabi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? (Environment.GetEnvironmentVariable("PGHOST") is not null ? BuildConnectionStringFromEnv() : null)
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DB接続情報が未設定です。");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

static string BuildConnectionStringFromEnv()
{
    var host = Environment.GetEnvironmentVariable("PGHOST");
    var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
    var db   = Environment.GetEnvironmentVariable("PGDATABASE");
    var user = Environment.GetEnvironmentVariable("PGUSER");
    var pass = Environment.GetEnvironmentVariable("PGPASSWORD");
    if (host is null || db is null || user is null || pass is null)
        throw new InvalidOperationException("DB接続情報が設定されていません。ConnectionStrings__DefaultConnection または PG* 環境変数を設定してください。");
    return $"Host={host};Port={port};Database={db};Username={user};Password={pass}";
}

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<AccountDeletionService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
    };
    // SignalR はクエリパラメータでトークンを渡す
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) &&
                ctx.HttpContext.Request.Path.StartsWithSegments("/hubs/chat"))
                ctx.Token = token;
            return Task.CompletedTask;
        }
    };
});

// SignalR の UserIdentifier を JWT の "sub" クレームで解決する
builder.Services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        var origins = (builder.Configuration["ClientUrl"] ?? "http://localhost:5048")
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (args.Length > 0 && args[0] == "cleanup-test-accounts")
{
    return await CleanupTestAccountsCommand.RunAsync(args, app.Services);
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapFallbackToFile("index.html");

app.Run();
return 0;
