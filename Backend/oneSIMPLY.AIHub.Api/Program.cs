using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using oneSIMPLY.AIHub.Api.Data;
using oneSIMPLY.AIHub.Api.Models;
using oneSIMPLY.AIHub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Secret trong appsettings.json");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "oneSIMPLY.AIHub";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "oneSIMPLY.AIHub.Client";
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=BINHNV\\SQL2022;Database=oneSIMPLY_TH;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(dbConnectionString));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILLMService, LLMService>();

builder.Services.AddControllers();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AIHubFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Logs"));
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Uploads"));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AIHubFrontend");
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "Uploads")),
    RequestPath = "/uploads"
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!await db.Subscriptions.AnyAsync())
    {
        db.Subscriptions.AddRange(
            new Subscription { Name = "Free Trial", Price = 0, MaxRequests = 10, DurationDays = 30 },
            new Subscription { Name = "Starter", Price = 390000, MaxRequests = 50, DurationDays = 30 },
            new Subscription { Name = "Professional", Price = 990000, MaxRequests = 200, DurationDays = 30 }
        );
        await db.SaveChangesAsync();
    }
}

app.Run();
