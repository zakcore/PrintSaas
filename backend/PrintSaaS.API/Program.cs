using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PrintSaaS.API.Hubs;
using PrintSaaS.Core.Services;
using PrintSaaS.Data;
using PrintSaaS.Data.Repositories;
using PrintSaaS.Engine;
using PrintSaaS.Rules;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<PrintContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
        }));
builder.Services.AddHangfireServer();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "development-key-min-32-chars-long!!"))
        };

        // Allow SignalR to receive token from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IPrinterRepository, PrinterRepository>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Core Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IPrinterService, PrinterService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUserService, UserService>();

// Engine Services
builder.Services.AddScoped<IIppPrintSender, IppPrintSender>();
builder.Services.AddScoped<IPrinterMonitor, PrinterMonitor>();
builder.Services.AddScoped<IFileEncryptionService, FileEncryptionService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IJobProcessor, JobProcessor>();
builder.Services.AddScoped<IPrinterProfileDiscovery, PrinterProfileDiscovery>();

// Rules Engine
builder.Services.AddScoped<IPrintRulesEngine, PrintRulesEngine>();

// Background Service — print engine polling loop
builder.Services.AddHostedService<PrintEngineService>();

// SignalR
builder.Services.AddSignalR();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

// CORS — allow frontend dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-migrate and seed in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PrintContext>();
    await db.Database.MigrateAsync();

    // Seed admin password on first run
    var admin = await db.Users.FindAsync(1);
    if (admin is not null && admin.PasswordHash.StartsWith("$2a$11$placeholder"))
    {
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234");
        await db.SaveChangesAsync();
    }

    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PrintStatusHub>("/hubs/print-status");

// Hangfire Dashboard (Admin only in production)
app.MapHangfireDashboard("/hangfire");

// Hangfire Recurring Jobs
RecurringJob.AddOrUpdate<IPrinterMonitor>(
    "monitor-printers", m => m.MonitorAllPrintersAsync(), "*/1 * * * *"); // Every minute

RecurringJob.AddOrUpdate<IJobProcessor>(
    "check-paper-levels", j => j.CheckPaperLevelsAsync(), "*/5 * * * *"); // Every 5 min

RecurringJob.AddOrUpdate<IJobProcessor>(
    "check-job-errors", j => j.CheckJobErrorsAsync(), "*/5 * * * *"); // Every 5 min

RecurringJob.AddOrUpdate<IJobProcessor>(
    "cleanup-retain-queue", j => j.CleanupRetainQueueAsync(), "0 0 * * *"); // Daily midnight

app.Run();
