using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Models.API;
using Bmw.Dashboard.Core.Models.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Bmw.Dashboard.Core.Services;

public class BmwApiService : IBmwApiService
{
    private readonly HttpClient _http;
    private readonly ISettingsService _settings;
    private readonly BmwApiOptions _options;
    private readonly ILogger<BmwApiService> _logger;
    private const string baseUrl = "https://api-cardata.bmwgroup.com/";
    private string _clientId => _settings.ClientId ?? _options.ClientId;

    public BmwApiService(HttpClient httpClient, ISettingsService settingsService, IOptions<BmwApiOptions> options, ILogger<BmwApiService> logger)
    {
        _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _settings = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _options = options?.Value ?? new BmwApiOptions();
        _logger = logger;
    }

    public async Task<TokenResponse?> GetAccessTokenFromRefresh(string refreshToken)
    {
        using var client = new HttpClient();

        var body = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("client_id", _clientId)
        ]);

        try
        {
            var response = await client.PostAsync("https://customer.bmwgroup.com/gcdm/oauth/token", body);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TokenResponse>();
            }
            else
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to refresh token: {response.StatusCode} - {errorDetails}");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetAccessTokenFromRefresh");
            return null;
        }
    }

    public async Task<DeviceCodeResponse?> GetDeviceCodeAsync(string challenge)
    {
        var uri = "https://customer.bmwgroup.com/gcdm/oauth/device/code";
        Debug.WriteLine($"GetDeviceCodeAsync start; uri={uri}; challengeLen={challenge.Length}");

        var body = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("response_type", "device_code"),
            new KeyValuePair<string, string>("scope", "authenticate_user openid cardata:api:read cardata:streaming:read"),
            new KeyValuePair<string, string>("code_challenge", challenge),
            new KeyValuePair<string, string>("code_challenge_method", "S256"),
        ]);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            Debug.WriteLine("Posting device code request to " + uri);
            _logger?.LogDebug("Posting device code request to {Uri}", uri);
            var response = await _http.PostAsync(uri, body, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DeviceCodeResponse>(cancellationToken: cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException oce) when (cts.IsCancellationRequested)
        {
            _logger?.LogError(oce, "GetDeviceCodeAsync timed out after {Timeout}s", 15);
            Debug.WriteLine($"GetDeviceCodeAsync timed out: {oce.Message}");
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in GetDeviceCodeAsync");
            Debug.WriteLine($"GetDeviceCodeAsync error: {ex}");
            return null;
        }
    }

    public async Task<HttpResponseMessage> GetTokenAsync(string deviceCode, string codeVerifier)
    {
        var uri = "https://customer.bmwgroup.com/gcdm/oauth/token";
        var body = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("device_code", deviceCode),
            new KeyValuePair<string, string>("code_verifier", codeVerifier),
        ]);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            Debug.WriteLine("Posting token request to " + uri);
            _logger?.LogDebug("Posting token request to {Uri}", uri);
            var response = await _http.PostAsync(uri, body, cts.Token).ConfigureAwait(false);
            _logger?.LogDebug("Token request returned {Status}", response.StatusCode);
            return response;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogError("GetTokenAsync timed out");
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetTokenAsync failed");
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }

    public async Task<TokenResponse?> PollForTokenAsync(string deviceCode, string codeVerifier, int interval)
    {
        var stopTime = DateTime.UtcNow.AddMinutes(5); // Set a reasonable timeout for polling

        while (DateTime.UtcNow < stopTime)
        {
            await Task.Delay(TimeSpan.FromSeconds(interval));

            var response = await GetTokenAsync(deviceCode, codeVerifier);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<TokenResponse>(json);

                if (tokenData == null)
                {
                    _logger.LogError("Failed to deserialize token response: {Json}", json);
                    throw new Exception("Failed to deserialize token response");
                }

                return tokenData;
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            if (!errorResponse.Contains("authorization_pending"))
            {
                _logger.LogError("Error polling for token: {ErrorResponse}", errorResponse);
                throw new Exception($"Error polling for token: {errorResponse}");
            }
        }

        _logger.LogError("Polling for token timed out after 5 minutes");
        return null;
    }

    public async Task<string> GetCarData(string accessToken)
    {
        string cleanVin = (_settings.VehicleVin ?? _options.VIN).Trim().ToUpper();
        // Ensure you use the modern URL found in Swagger
        //string url = $"https://api-cardata.bmwgroup.com/customers/vehicles/{cleanVin}/basicData";
        string url = $"https://api-cardata.bmwgroup.com/customers/vehicles/mappings";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // Use TryAddWithoutValidation to prevent the HttpClient from 
        // potentially modifying the header format
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-version", "v1");

        // REQUIRED: Mimic a real browser identity
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) BmwDashboard/1.0");

        // REQUIRED: Add the language header
        request.Headers.Add("X-Language", "en-GB");

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            // Check if this body mentions "CONSENT_MISSING" or "IDENTIFIER_UNKNOWN"
            Debug.WriteLine($"Status: {response.StatusCode}, Body: {errorBody}");
        }

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<IEnumerable<VehicleMappingResponse>> GetVehicleMappings(string accessToken)
    {
        var fullUrl = $"{baseUrl}customers/vehicles/mappings";
        using var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);

        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("x-version", "v1");
        request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) BmwDashboard/1.0");
        request.Headers.Add("X-Language", "en-GB");

        var response = await _http.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<List<VehicleMappingResponse>>(content, options);

            return result ?? Enumerable.Empty<VehicleMappingResponse>();

        }
        else
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get vehicle mappings. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
            throw new Exception($"Failed to get vehicle mappings. Status: {response.StatusCode}, Body: {errorBody}");
        }
    }
}
