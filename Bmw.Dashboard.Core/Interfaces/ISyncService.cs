using Bmw.Dashboard.Core.Models.API;

namespace Bmw.Dashboard.Core.Interfaces;

public interface ISyncService
{
    //auth methods
    Task<DeviceCodeResponse?> GetDeviceCodeAsync();
    Task<bool> PollForTokenAsync(string deviceCode, int interval);
    Task<bool> GetTokenFromRefresh(string refreshToken);

    //car data methods
    Task<string> GetCarData();
    Task<IEnumerable<VehicleMappingResponse>> GetVehicleMappings();
    Task<string?> GetOrFetchVehicleImagePathAsync(string vin);
}
