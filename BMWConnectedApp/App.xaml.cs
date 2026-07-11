using Bmw.Dashboard.Core.Data.DbContexts;
using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Models.Config;
using Bmw.Dashboard.Core.Services;
using Bmw.Dashboard.Core.ViewModels;
using BMWConnectedApp.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using WinRT.BMWConnectedAppVtableClasses;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BMWConnectedApp;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static IHost AppHost { get; private set; } = default!;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        // Build the generic host and register services. Heavy initialization that requires
        // the DI container (like ensuring DB creation or loading settings) will be performed
        // asynchronously in OnLaunched so startup remains async-friendly.
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                // Ensure debug-level logs are written to the VS Output (Debug) window
                logging.ClearProviders();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            })
            .ConfigureServices((context, services) =>
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "bmw_dashboard.db");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string dbPath = Path.Combine(folderPath, "bmw_dashboard.db");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            services.Configure<BmwApiOptions>(context.Configuration.GetSection("BmwApi"));

            services.AddHttpClient<IBmwApiService, BmwApiService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<DataSyncService>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddSingleton<IPasswordVaultService, PasswordVaultService>();
        }).Build();

        // Global handler to catch unhandled exceptions on the UI thread for diagnostics
        this.UnhandledException += (sender, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"UnhandledException: {e.Exception.Message} \n{e.Exception.StackTrace}");
            // Allow default handling after logging
        };
    }

    public static T GetService<T>() where T : class => AppHost.Services.GetService(typeof(T)) as T;
    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            using var scope = AppHost.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // Ensure database exists (async)
            await dbContext.Database.EnsureCreatedAsync();

            // Load settings into cache
            var settingsService = scope.ServiceProvider.GetRequiredService<Bmw.Dashboard.Core.Interfaces.ISettingsService>();
            await settingsService.LoadAsync();

            // Quick connectivity test using the API service to help diagnose hangs
            try
            {
                var api = scope.ServiceProvider.GetRequiredService<Bmw.Dashboard.Core.Interfaces.IBmwApiService>();
                var ok = await api.TestConnectivityAsync();
                System.Diagnostics.Debug.WriteLine($"Connectivity test result: {ok}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connectivity test error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            // Log the error but do not abort startup; proceed to show main window
            System.Diagnostics.Debug.WriteLine($"Startup init error: {ex.Message}");
        }

        m_window = new MainWindow();
        m_window.Activate();
    }

    public static bool TryGoBack()
    {
        Frame? rootFrame = Window.Content as Frame;
        if(rootFrame == null) return false;
        if(rootFrame.CanGoBack)
        {
            rootFrame.GoBack();
            return true;
        }
        return false;
    }

    public static Window Window { get { return m_window; } }
    private static Window m_window;
}
