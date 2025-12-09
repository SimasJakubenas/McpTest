using System.ComponentModel;
using System.Text.Json;
using McpTest.Models;
using McpTest.Services;
using ModelContextProtocol.Server;

namespace McpTest.Tools;
/// <summary>
/// MCP tools for interacting with Progress Application Server OpenEdge (PASOE) via REST.
/// </summary>
public class PasoeRestTools
{
    private readonly IPasoeRestClient _pasoeClient;

    public PasoeRestTools(IPasoeRestClient pasoeClient)
    {
        _pasoeClient = pasoeClient;
    }
    
    [McpServerTool]
    [Description("Calls a PASOE REST service endpoint with the specified parameters.")]
    public async Task<string> CallPasoeService(
        [Description("The service path (e.g., '/rest/CustomersService/MyMethod')")] string servicePath,
        [Description("HTTP method: GET, POST, PUT, DELETE (default: POST)")] string httpMethod = "POST",
        [Description("Request body as JSON string (optional)")] string? requestBody = null,
        [Description("Additional headers as JSON object (optional, e.g., '{\"X-Custom-Header\": \"value\"}')")] string? headers = null)
    {
        try
        {
            var request = new PasoeRequest
            {
                ServicePath = servicePath,
                HttpMethod = httpMethod,
                RequestBody = requestBody
            };


            if (!string.IsNullOrEmpty(headers))
            {
                var headerDict = JsonSerializer.Deserialize<Dictionary<string, string>>(headers);
                if (headerDict != null)
                {
                    request.Headers = headerDict;
                }
            }

            var response = await _pasoeClient.SendRequestAsync(request);

            var result = new
            {
                success = response.IsSuccess,
                statusCode = response.StatusCode,
                response = response.ResponseBody,
                errorMessage = response.ErrorMessage,
                headers = response.Headers
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 0,
                errorMessage = $"Error calling PASOE service: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
    
    [McpServerTool]
    [Description("Tests the connection to the configured PASOE instance.")]
    public async Task<string> TestPasoeConnection()
    {
        try
        {
            var isConnected = await _pasoeClient.TestConnectionAsync();


            var result = new
            {
                success = isConnected,
                message = isConnected
                    ? "Successfully connected to PASOE"
                    : "Failed to connect to PASOE"
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Connection test error: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
    
    [McpServerTool]
    [Description("Makes a GET request to retrieve data from a PASOE REST service.")]
    public async Task<string> GetPasoeData(
        [Description("The service path (e.g., '/rest/CustomersService/Customer')")] string servicePath,
        [Description("Query parameters as JSON object (optional, e.g., '{\"id\": \"123\"}')")] string? queryParams = null)
    {
        try
        {
            var url = servicePath;
            if (!string.IsNullOrEmpty(queryParams))
            {
                var paramDict = JsonSerializer.Deserialize<Dictionary<string, string>>(queryParams);
                if (paramDict != null && paramDict.Count > 0)
                {
                    var queryString = string.Join("&",
                        paramDict.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                    url += (servicePath.Contains('?') ? "&" : "?") + queryString;
                }
            }


            var request = new PasoeRequest
            {
                ServicePath = url,
                HttpMethod = "GET"
            };

            var response = await _pasoeClient.SendRequestAsync(request);

            var result = new
            {
                success = response.IsSuccess,
                statusCode = response.StatusCode,
                data = response.ResponseBody,
                errorMessage = response.ErrorMessage
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 0,
                errorMessage = $"Error retrieving data: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
    
    [McpServerTool]
    [Description("Makes a POST request to invoke an ABL procedure or create data in PASOE.")]
    public async Task<string> InvokePasoeMethod(
        [Description("The service path (e.g., '/rest/CustomersService/Customer')")] string servicePath,
        [Description("Request payload as JSON string")] string requestPayload)
    {
        try
        {
            var request = new PasoeRequest
            {
                ServicePath = servicePath,
                HttpMethod = "POST",
                RequestBody = requestPayload
            };


            var response = await _pasoeClient.SendRequestAsync(request);

            var result = new
            {
                success = response.IsSuccess,
                statusCode = response.StatusCode,
                result = response.ResponseBody,
                errorMessage = response.ErrorMessage
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 0,
                errorMessage = $"Error invoking method: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
    
    [McpServerTool]
    [Description("Makes a PUT request to update data in PASOE.")]
    public async Task<string> UpdatePasoeData(
        [Description("The service path (e.g., '/rest/CustomersService/Customer/123')")] string servicePath,
        [Description("Update payload as JSON string")] string requestPayload)
    {
        try
        {
            var request = new PasoeRequest
            {
                ServicePath = servicePath,
                HttpMethod = "PUT",
                RequestBody = requestPayload
            };


            var response = await _pasoeClient.SendRequestAsync(request);

            var result = new
            {
                success = response.IsSuccess,
                statusCode = response.StatusCode,
                result = response.ResponseBody,
                errorMessage = response.ErrorMessage
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 0,
                errorMessage = $"Error updating data: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
    
    [McpServerTool]
    [Description("Makes a DELETE request to remove data in PASOE.")]
    public async Task<string> DeletePasoeData(
        [Description("The service path (e.g., '/rest/CustomersService/Customer/123')")] string servicePath)
    {
        try
        {
            var request = new PasoeRequest
            {
                ServicePath = servicePath,
                HttpMethod = "DELETE"
            };


            var response = await _pasoeClient.SendRequestAsync(request);

            var result = new
            {
                success = response.IsSuccess,
                statusCode = response.StatusCode,
                result = response.ResponseBody,
                errorMessage = response.ErrorMessage
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                statusCode = 0,
                errorMessage = $"Error deleting data: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
