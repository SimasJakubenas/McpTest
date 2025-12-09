using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using McpTest.Models;
using McpTest.Services;
using McpTest.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = Host.CreateApplicationBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Load PASOE configuration from appsettings.json
builder.Services.Configure<PasoeConfiguration>(
    builder.Configuration.GetSection("Pasoe"));

// Register HttpClient for PASOE with SSL certificate validation options
builder.Services.AddHttpClient<IPasoeRestClient, PasoeRestClient>()
    .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
    {
        var config = serviceProvider.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<PasoeConfiguration>>().Value;

        var handler = new HttpClientHandler();

        // Allow invalid SSL certificates if configured (for development/testing)
        if (config.AcceptInvalidSslCertificates)
        {
            handler.ServerCertificateCustomValidationCallback =
                (HttpRequestMessage message, X509Certificate2? cert, X509Chain? chain, SslPolicyErrors errors) =>
                {
                    // WARNING: This accepts ALL certificates, including invalid ones
                    // Only use this in development/testing environments
                    return true;
                };
        }

        return handler;
    });

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<PasoeRestTools>();

await builder.Build().RunAsync();
