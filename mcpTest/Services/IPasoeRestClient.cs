using yourServerName.Models; 


namespace yourServerName.Services;

/// <summary>
/// Interface for PASOE REST client service.
/// </summary>
public interface IPasoeRestClient
{
    /// <summary>
    /// Sends a request to a PASOE REST service.
    /// </summary>
    /// <param name="request">The request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the PASOE service.</returns>
    Task<PasoeResponse> SendRequestAsync(PasoeRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to the PASOE instance.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if connection is successful, false otherwise.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
