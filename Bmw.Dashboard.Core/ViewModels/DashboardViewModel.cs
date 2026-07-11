using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bmw.Dashboard.Core.ViewModels;
public partial class DashboardViewModel(DataSyncService syncService, IPasswordVaultService passwordVaultService) : ObservableObject
{
    [ObservableProperty] private string? _userCode;
    [ObservableProperty] private bool _isWaitingForAuth;
    [ObservableProperty] private bool _isConnected;
    [ObservableProperty] private string _connectionStatusText = "Disconnected";

    [RelayCommand]
    public async Task InitialiseDashboard()
    {
        var refreshToken = passwordVaultService.GetRefreshToken();

        if(string.IsNullOrEmpty(refreshToken))
        {
            await AuthenticateAsync();
            return;
        }

        if (syncService.HasCachedToken)
        {
            IsConnected = true;
            ConnectionStatusText = "Connected";
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
        await syncService.GetVehicleMappings();
        return;
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
                }
                else
                {
                    UserCode = "Auth Failed";
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