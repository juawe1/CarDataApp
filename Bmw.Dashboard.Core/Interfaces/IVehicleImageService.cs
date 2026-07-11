namespace Bmw.Dashboard.Core.Interfaces;

public interface IVehicleImageService
{
    /// <summary>
    /// Returns the local file path for the vehicle image.
    /// If the image is not cached on disk, fetches it from the API and stores it first.
    /// Returns null if the image cannot be retrieved.
    /// </summary>
    Task<string?> GetOrFetchImagePathAsync(string accessToken, string vin);

    /// <summary>
    /// Returns true if a cached image already exists for the given VIN.
    /// </summary>
    bool IsCached(string vin);
}
