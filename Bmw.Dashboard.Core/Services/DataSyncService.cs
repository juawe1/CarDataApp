using Bmw.Dashboard.Core.Helpers;
using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Models.API;

namespace Bmw.Dashboard.Core.Services;

public class DataSyncService(IBmwApiService api, IPasswordVaultService passwordVaultService, IVehicleImageService vehicleImageService) : ISyncService
{
    public string? CachedAccessToken { get; set; }
    private string? _currentVerifier;

    public bool HasCachedToken => !string.IsNullOrEmpty(CachedAccessToken);
    public async Task<DeviceCodeResponse?> GetDeviceCodeAsync()
    {
        var (verifier, challenge) = PkceHelper.Generate();
        _currentVerifier = verifier;
        var response = await api.GetDeviceCodeAsync(challenge);
        return response ?? null;
    }
    public async Task<bool> PollForTokenAsync(string deviceCode, int interval)
    {
        var tokenData = await api.PollForTokenAsync(deviceCode, _currentVerifier!, interval);
        
        if(tokenData != null && !string.IsNullOrEmpty(tokenData.AccessToken))
        {
            this.CachedAccessToken = tokenData.AccessToken;
            passwordVaultService.SaveTokens(refreshToken: tokenData.RefreshToken);
            return true;
        }
        return false;
    }

    public async Task<bool> GetTokenFromRefresh(string refreshToken)
    {
        try
        {
            var response = await api.GetAccessTokenFromRefresh(refreshToken);
            if (response is not null)
            {
                if (!string.IsNullOrWhiteSpace(response.AccessToken)) this.CachedAccessToken = response.AccessToken;
                passwordVaultService.SaveTokens(response.RefreshToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetTokenFromRefresh error: {ex}");
            return false;
        }
    }

    public async Task<string> GetCarData()
    {
        if (CachedAccessToken is null) return string.Empty;
        var response = await api.GetCarData(CachedAccessToken);
        return response;
    }

    public async Task<IEnumerable<VehicleMappingResponse>> GetVehicleMappings()
    {
        if (CachedAccessToken is null) return [];
        var response = await api.GetVehicleMappings(CachedAccessToken);
        return response;
    }

    public async Task<string?> GetOrFetchVehicleImagePathAsync(string vin)
    {
        if (CachedAccessToken is null) return null;
        return await vehicleImageService.GetOrFetchImagePathAsync(CachedAccessToken, vin);
    }
}
