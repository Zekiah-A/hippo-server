using System.Text.Json.Serialization;

namespace HippoServer;

public class ServerConfig
{
    [JsonIgnore] public const int LatestVersion = 1;
    public int Version { get; set; } = LatestVersion;
    public int Port { get; set; } = 8080;
    public bool UseHttps { get; set; } = false;
    public string CertPath { get; set; } = string.Empty;
    public string KeyPath { get; set; } = string.Empty;
    
    // Mail
    public string SmtpHost { get; set; } = "";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public string SmtpFromEmail { get; set; } = "no-reply@hippo.casino";
    public bool SmtpUseSsl { get; set; } = true;
}