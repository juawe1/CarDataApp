using Bmw.Dashboard.Core.Data.Entities;
using Bmw.Dashboard.Core.Helpers;
using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bmw.Dashboard.Core.ViewModels;

public class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IBmwApiService _api;
    private readonly IPasswordVaultService _passwordVaultService;
    private readonly DataSyncService _syncService;
    private string? _codeVerifier;

    private UserConfigEntity _userConfig = new();
    public UserConfigEntity UserConfig
    {
        get => _userConfig;
        set => SetProperty(ref _userConfig, value);
    }

    private string _statusText = string.Empty;
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string _deviceVerificationUri = string.Empty;
    public string DeviceVerificationUri
    {
        get => _deviceVerificationUri;
        set => SetProperty(ref _deviceVerificationUri, value);
    }

    private string _userCode = string.Empty;
    public string UserCode
    {
        get => _userCode;
        set => SetProperty(ref _userCode, value);
    }

    private bool _isAuthorized = false;
    public bool IsAuthorized
    {
        get => _isAuthorized;
        set => SetProperty(ref _isAuthorized, value);
    }

    private bool _isPolling = false;
    public bool IsPolling
    {
        get => _isPolling;
        set => SetProperty(ref _isPolling, value);
    }

    public SettingsViewModel(ISettingsService settingsService, IBmwApiService api, IPasswordVaultService passwordVaultService, DataSyncService syncService)
    {
        _settingsService = settingsService;
        _api = api;
        _passwordVaultService = passwordVaultService;
        _syncService = syncService;
    }

    public async Task LoadAsync()
    {
        await _settingsService.LoadAsync();
        UserConfig = _settingsService.Current ?? new UserConfigEntity();
    }

    public async Task SaveAsync()
    {
        try
        {
            await _settingsService.SaveAsync(UserConfig);
            StatusText = "Saved";
        }
        catch
        {
            StatusText = "Save failed";
        }
    }

    public async Task AuthorizeAsync()
    {
        try
        {
            var pkce = PkceHelper.Generate();
            _codeVerifier = pkce.verifier;
            var deviceResp = await _api.GetDeviceCodeAsync(pkce.challenge);
            if (deviceResp == null)
            {
                StatusText = "Failed to start authorization";
                return;
            }

            DeviceVerificationUri = deviceResp.VerificationUri ?? string.Empty;
            UserCode = deviceResp.UserCode ?? string.Empty;
            IsPolling = true;
            StatusText = "Waiting for user to authorize...";

            var token = await _api.PollForTokenAsync(deviceResp.DeviceCode, _codeVerifier!, deviceResp.Interval);
            IsPolling = false;

            if (token != null)
            {
                _passwordVaultService.SaveTokens(token.RefreshToken ?? string.Empty);
                IsAuthorized = true;
                StatusText = "Authorized";
            }
            else
            {
                StatusText = "Authorization not completed";
            }
        }
        catch
        {
            StatusText = "Authorization failed";
            IsPolling = false;
        }
    }

    public async Task<List<ContainerEntity>> QueryContainersAsync()
    {
        
        var containers = await _syncService.GetContainers();
        if (containers != null && containers.Count != 0)
        {
            return containers;
        }
        else
        {
            return [];
        }
    }
}
