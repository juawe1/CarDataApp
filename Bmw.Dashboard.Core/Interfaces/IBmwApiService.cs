using Bmw.Dashboard.Core.Models.API;

namespace Bmw.Dashboard.Core.Interfaces;

public interface IBmwApiService
{
    //auth methods
    Task<DeviceCodeResponse?> GetDeviceCodeAsync(string challenge);
    Task<HttpResponseMessage> GetTokenAsync(string deviceCode, string codeVerifier);
    Task<TokenResponse?> PollForTokenAsync(string deviceCode, string codeVerifier, int interval);
    Task<TokenResponse?> GetAccessTokenFromRefresh(string refreshToken);

    //car data methods
    Task<string> GetCarData(string accessToken);
    Task<IEnumerable<VehicleMappingResponse>> GetVehicleMappings(string accessToken);
}
