
using System.Text.Json.Serialization;

namespace Bmw.Dashboard.Core.Models.API;

public class DeviceCodeResponse
{
    [JsonPropertyName("user_code")] public string UserCode { get; set; }
    [JsonPropertyName("device_code")] public string DeviceCode { get; set; }
    [JsonPropertyName("verification_uri")] public string VerificationUri { get; set; }
    [JsonPropertyName("interval")] public int Interval { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

public class  TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; }
    [JsonPropertyName("error")] public string Error { get; set; }
}
