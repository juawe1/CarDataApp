using Bmw.Dashboard.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Bmw.Dashboard.Core.Services;

public class VehicleImageService : IVehicleImageService
{
    private readonly IBmwApiService _api;
    private readonly ILogger<VehicleImageService> _logger;
    private readonly string _cacheDirectory;

    public VehicleImageService(IBmwApiService api, ILogger<VehicleImageService> logger)
    {
        _api = api;
        _logger = logger;
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BMWDashboard",
            "images");

        Directory.CreateDirectory(_cacheDirectory);
    }

    public bool IsCached(string vin)
    {
        try
        {
            return File.Exists(GetCachePath(vin));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid VIN format: {Vin}", vin);
            return false;
        }
    }

    public async Task<string?> GetOrFetchImagePathAsync(string accessToken, string vin)
    {
        string path;
        string normalizedVin = vin.Trim().ToUpperInvariant();

        try
        {
            path = GetCachePath(normalizedVin);

            if (File.Exists(path))
            {
                _logger.LogDebug("Vehicle image cache hit for VIN {Vin}", vin);
                return path;
            }

            _logger.LogInformation("Fetching vehicle image from API for VIN {Vin}", vin);
            var imageBytes = await _api.GetVehicleImageAsync(accessToken, vin);

            if (imageBytes is null || imageBytes.Length == 0)
            {
                _logger.LogWarning("No image data returned for VIN {Vin}", vin);
                return null;
            }

            await File.WriteAllBytesAsync(path, imageBytes);
            _logger.LogInformation("Vehicle image cached at {Path}", path);
            return path;
        }
        catch (Exception ex) 
        { 
            _logger.LogError(ex, "Error fetching vehicle image for VIN {Vin}", vin);
            return null;
        }
    }

    private string GetCachePath(string vin)
    {
        var normalised = vin.Trim().ToUpperInvariant();

        if(normalised.Length != 17) 
            throw new ArgumentException("VIN must be 17 characters long.", nameof(vin));
        
        foreach(var ch in normalised)
        {
            if(!char.IsLetterOrDigit(ch))
                throw new ArgumentException("VIN must only contain alphanumeric characters.", nameof(vin));
        }

        return Path.Combine(_cacheDirectory, $"{normalised}.jpg");
    }
}
