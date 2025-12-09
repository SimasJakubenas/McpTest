namespace yourServerName.Models;

/// <summary>
/// Represents a response from a PASOE REST service call.
/// </summary>
public class PasoeResponse
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// The HTTP status code returned.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// The response body as a string.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Error message if the request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Response headers.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();
}
