using System.Net;
using System.Net.Http.Headers;
using System.Text;
using McpTest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace McpTest.Services;

/// <summary>
/// Service for communicating with PASOE REST endpoints.
/// </summary>
public class PasoeRestClient : IPasoeRestClient
{
    private readonly HttpClient _httpClient;
    private readonly PasoeConfiguration _config;
    private readonly ILogger<PasoeRestClient> _logger;

    public PasoeRestClient(
        HttpClient httpClient,
        IOptions<PasoeConfiguration> config,
        ILogger<PasoeRestClient> logger)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        if (!string.IsNullOrEmpty(_config.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

        if (!string.IsNullOrEmpty(_config.Username) && !string.IsNullOrEmpty(_config.Password))
        {
            var credentials = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_config.Username}:{_config.Password}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<PasoeResponse> SendRequestAsync(
        PasoeRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Sending {Method} request to PASOE: {ServicePath}",
                request.HttpMethod,
                request.ServicePath);

            var url = BuildServiceUrl(request.ServicePath);

            var httpRequest = new HttpRequestMessage(
                new HttpMethod(request.HttpMethod.ToUpper()),
                url);

            foreach (var header in request.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (!string.IsNullOrEmpty(request.RequestBody))
            {
                httpRequest.Content = new StringContent(
                    request.RequestBody,
                    Encoding.UTF8,
                    "application/json");
            }

            var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);

            var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            var response = new PasoeResponse
            {
                IsSuccess = httpResponse.IsSuccessStatusCode,
                StatusCode = (int)httpResponse.StatusCode,
                ResponseBody = responseBody
            };

            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                response.ErrorMessage = $"HTTP {response.StatusCode}: {httpResponse.ReasonPhrase}";
                _logger.LogWarning(
                    "PASOE request failed with status {StatusCode}: {ReasonPhrase}",
                    response.StatusCode,
                    httpResponse.ReasonPhrase);
            }
            else
            {
                _logger.LogInformation("PASOE request completed successfully");
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed");
            return new PasoeResponse
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = $"HTTP request failed: {ex.Message}"
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request timed out");
            return new PasoeResponse
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = "Request timed out"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during PASOE request");
            return new PasoeResponse
            {
                IsSuccess = false,
                StatusCode = 0,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Testing connection to PASOE at {BaseUrl}", _config.BaseUrl);

            var response = await _httpClient.GetAsync(
                BuildServiceUrl(""),
                cancellationToken);

            var isConnected = response.StatusCode != HttpStatusCode.NotFound &&
                              response.StatusCode != HttpStatusCode.ServiceUnavailable;

            _logger.LogInformation(
                "Connection test result: {IsConnected} (Status: {StatusCode})",
                isConnected,
                response.StatusCode);

            return isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed");
            return false;
        }
    }

    private string BuildServiceUrl(string servicePath)
    {
        if (string.IsNullOrEmpty(_config.BaseUrl))
        {
            throw new InvalidOperationException(
                "PASOE BaseUrl is not configured. Please check appsettings.json Pasoe:BaseUrl setting.");
        }

        if (!string.IsNullOrEmpty(servicePath) && !servicePath.StartsWith('/'))
        {
            servicePath = "/" + servicePath;
        }

        var url = $"{_config.BaseUrl.TrimEnd('/')}/{_config.WebAppName.Trim('/')}{servicePath}";
       
        _logger.LogDebug("Built service URL: {Url}", url);
       
        return url;
    }
}
