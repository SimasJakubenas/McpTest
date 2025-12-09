namespace McpTest.Models;


/// <summary>
/// Represents a request to call an ABL procedure via PASOE REST.
/// </summary>
public class PasoeRequest
{
    /// <summary>
    /// The service path (e.g., "/rest/MyService").
    /// </summary>
    public string ServicePath { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP method to use (GET, POST, PUT, DELETE).
    /// </summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// The request body (typically JSON for REST services).
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Additional headers to include in the request.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}
