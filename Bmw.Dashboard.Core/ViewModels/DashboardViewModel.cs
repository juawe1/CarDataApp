using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Models;
using Bmw.Dashboard.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Bmw.Dashboard.Core.ViewModels;
public partial class DashboardViewModel(DataSyncService syncService, IPasswordVaultService passwordVaultService) : ObservableObject
{
    [ObservableProperty] private string? _userCode;
    [ObservableProperty] private bool _isWaitingForAuth;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _connectionStatusText = "Disconnected";
    [ObservableProperty] private bool _isLoading;

    private const string PlaceholderImageUrl =
        "https://images.unsplash.com/photo-1503376780353-7e6692767b70?q=80&w=1000";

    [ObservableProperty] private string _vehicleImageSource = PlaceholderImageUrl;

    public ObservableCollection<VehicleDisplayModel> Vehicles { get; } = [];

    [RelayCommand]
    public async Task InitialiseDashboard()
    {
        var refreshToken = passwordVaultService.GetRefreshToken();

        if (string.IsNullOrEmpty(refreshToken))
        {
            await AuthenticateAsync();
            return;
        }

        if (syncService.HasCachedToken)
        {
            IsConnected = true;
            ConnectionStatusText = "Connected";
            await LoadVehicleDataAsync();
            return;
        }

        bool tokenRetrieved = await syncService.GetTokenFromRefresh(refreshToken);

        if (!tokenRetrieved)
        {
            IsWaitingForAuth = false;
            IsConnected = false;
            ConnectionStatusText = "Disconnected";
            return;
        }

        IsWaitingForAuth = false;
        IsConnected = true;
        ConnectionStatusText = "Connected";
        await LoadVehicleDataAsync();
    }

    private async Task LoadVehicleDataAsync()
    {
        IsLoading = true;
        Vehicles.Clear();

        try
        {
            var mappings = await syncService.GetVehicleMappings();

            // Fetch all vehicle images in parallel
            var imageTasks = mappings.Select(async m => new VehicleDisplayModel
            {
                Vin = m.Vin,
                MappingType = m.MappingType,
                ImagePath = await syncService.GetOrFetchVehicleImagePathAsync(m.Vin)
            });

            var vehicles = await Task.WhenAll(imageTasks);

            // Primary vehicle first, then secondary
            foreach (var vehicle in vehicles.OrderByDescending(v => v.IsPrimary))
                Vehicles.Add(vehicle);

            // Update hero image to the primary vehicle's cached image, if available
            var primaryImage = Vehicles.FirstOrDefault(v => v.IsPrimary)?.ImagePath
                            ?? Vehicles.FirstOrDefault()?.ImagePath;

            if (primaryImage != null)
                VehicleImageSource = new Uri(primaryImage).AbsoluteUri;
        }
        finally
        {
            IsLoading = false;
        }
    }
    public async Task AuthenticateAsync()
    {
        try
        {
            var deviceResponse = await syncService.GetDeviceCodeAsync();
            UserCode = deviceResponse?.UserCode;
            IsWaitingForAuth = true;

            if (deviceResponse != null)
            {
                bool success = await syncService.PollForTokenAsync(deviceResponse.DeviceCode, deviceResponse.Interval);

                if (success)
                {
                    IsWaitingForAuth = false;
                    IsConnected = true;
                    ConnectionStatusText = "Connected";
                    await LoadVehicleDataAsync();
                    return;
                }
            }

            UserCode = "Auth Failed";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"BMW Auth Error: {ex.Message}");
            UserCode = "Auth Failed";
        }
    }
}