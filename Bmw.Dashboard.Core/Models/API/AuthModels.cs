
using System.Text.Json.Serialization;

namespace Bmw.Dashboard.Core.Models.API;

public class DeviceCodeResponse
{
    [JsonPropertyName("user_code")] public string UserCode { get; set; } = string.Empty;
    [JsonPropertyName("device_code")] public string DeviceCode { get; set; } = string.Empty;
    [JsonPropertyName("verification_uri")] public string VerificationUri { get; set; } = string.Empty;
    [JsonPropertyName("interval")] public int Interval { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

public class  TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("error")] public string Error { get; set; } = string.Empty;
}
