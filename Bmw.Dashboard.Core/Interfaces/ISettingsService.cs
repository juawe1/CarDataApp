using Bmw.Dashboard.Core.Data.Entities;
using System.Threading.Tasks;

namespace Bmw.Dashboard.Core.Interfaces;

public interface ISettingsService
{
    /// <summary>
    /// Load settings from the underlying store (DB) into memory cache.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Save provided settings to the store and update cache.
    /// </summary>
    Task SaveAsync(UserConfigEntity config);

    /// <summary>
    /// Get cached client id (or null if not set).
    /// </summary>
    string? ClientId { get; }

    /// <summary>
    /// Get cached vehicle vin (or null if not set).
    /// </summary>
    string? VehicleVin { get; }

    /// <summary>
    /// Get the current cached UserConfigEntity (may be null until LoadAsync completes).
    /// </summary>
    UserConfigEntity? Current { get; }
}
