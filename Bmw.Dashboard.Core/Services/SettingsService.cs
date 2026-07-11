using Bmw.Dashboard.Core.Data.DbContexts;
using Bmw.Dashboard.Core.Data.Entities;
using Bmw.Dashboard.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Bmw.Dashboard.Core.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _dbContext;
    private UserConfigEntity? _cache;

    public SettingsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public UserConfigEntity? Current => _cache;

    public string? ClientId => _cache?.ClientId;

    public string? VehicleVin => _cache?.VehicleVin;

    public async Task LoadAsync()
    {
        // Load the first user config if present
        _cache = await _dbContext.UserConfigs.FirstOrDefaultAsync();
    }

    public async Task SaveAsync(UserConfigEntity config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));

        var existing = await _dbContext.UserConfigs.FirstOrDefaultAsync();
        if (existing != null)
        {
            existing.ClientId = config.ClientId;
            existing.VehicleVin = config.VehicleVin;
            existing.LastUpdated = DateTime.UtcNow;
            _dbContext.UserConfigs.Update(existing);
        }
        else
        {
            config.LastUpdated = DateTime.UtcNow;
            await _dbContext.UserConfigs.AddAsync(config);
        }

        await _dbContext.SaveChangesAsync();

        // Update cache
        _cache = config;
    }
}
