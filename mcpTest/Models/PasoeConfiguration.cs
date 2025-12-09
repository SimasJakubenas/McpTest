namespace McpTest.Models;

/// <summary>
/// Configuration settings for connecting to a Progress Application Server (PASOE) instance.
/// </summary>
public class PasoeConfiguration
{
    /// <summary>
    /// The base URL of the PASOE instance (e.g., "https://localhost:8810").
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// The web application name (e.g., "web" or your custom app name).
    /// </summary>
    public string WebAppName { get; set; } = "web";

    /// <summary>
    /// Username for basic authentication (if required).
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for basic authentication (if required).
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Timeout in seconds for HTTP requests (default: 30 seconds).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to accept invalid SSL certificates (use with caution, only for development).
    /// </summary>
    public bool AcceptInvalidSslCertificates { get; set; } = false;
}
