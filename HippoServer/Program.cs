using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace HippoServer;

internal static partial class Program
{
    private static ServerConfig? config;
    private static WebApplication app = null!;
    private static SmtpClient smtpClient = null!;
    
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var logger = builder.Logging.AddConsole().AddDebug();
        var dataDir = new DirectoryInfo("HippoData");
        var configFile = Path.Combine(dataDir.FullName, "hippo-config.json");
        var dbFile = Path.Combine(dataDir.FullName, "hippo-server.db");

        if (!Directory.Exists(dataDir.FullName))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARN]: Couldn't find the data directory. Creating new at {0}.", dataDir.FullName);
            Console.ResetColor();
            Directory.CreateDirectory(dataDir.FullName);
        }
        
        if (File.Exists(configFile))
        {
            var configText = await File.ReadAllTextAsync(configFile);
            config = JsonSerializer.Deserialize<ServerConfig>(configText);
        }

        if (config?.Version < ServerConfig.LatestVersion)
        {
            var configMoveLocation = configFile.Replace(".json", $".version{config.Version}.old.json");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARN]: Current config of version {0} older than current version {1}." +
                              "Outdated config file will be moved to {2}.",
                config.Version, ServerConfig.LatestVersion, configMoveLocation);
            Console.ResetColor();
            File.Move(configFile, configMoveLocation);
            config = null;
        }
        if (config is null)
        {
            await using var stream = File.OpenWrite(configFile);
            await JsonSerializer.SerializeAsync(stream, new ServerConfig(), new JsonSerializerOptions
            {
                WriteIndented = true,
            });
            await stream.FlushAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("[LOG]: New config created! Please edit {0} and run this program again!", configFile);
            Console.ResetColor();
            Environment.Exit(0);
        }
        
        // SMTP
        smtpClient = new SmtpClient(config.SmtpHost)
        {
            Port = config.SmtpPort,
            Credentials = new NetworkCredential(config.SmtpUser, config.SmtpPassword),
            EnableSsl = config.SmtpUseSsl,
        };
        
        // CORS and certs
        builder.Configuration["Kestrel:Certificates:Default:Path"] = config.CertPath;
        builder.Configuration["Kestrel:Certificates:Default:KeyPath"] = config.KeyPath;
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost/", "http://localhost:80", "https://hippo.casino")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        
        // Json
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });
        
        // Database
        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlite($"Data Source={dbFile}");
        });
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // Authentication
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        builder.Services.AddAuthorization();

        // Enable services
        app = builder.Build();
        app.Urls.Add($"{(config.UseHttps ? "https" : "http")}://*:{config.Port}");
        app.UseCors();
        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        MapAuthEndpoints();
        
        await app.RunAsync();
    }
}