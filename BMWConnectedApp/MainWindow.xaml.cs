using Bmw.Dashboard.Core.ViewModels;
using BMWConnectedApp.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace BMWConnectedApp;

public sealed partial class MainWindow : Window
{
    public DashboardViewModel ViewModel { get; }
    private bool _activatedOnce = false;

    public MainWindow()
    {
        System.Diagnostics.Debug.WriteLine("MainWindow ctor start");
        this.InitializeComponent();
        System.Diagnostics.Debug.WriteLine("MainWindow ctor after InitializeComponent");
        ViewModel = App.GetService<DashboardViewModel>();

        this.Activated += (s, e) =>
        {
            // Only perform initial navigation/initialisation once. Activated fires whenever the
            // window becomes active (e.g., when the user clicks away and returns), which caused
            // the app to always navigate back to Home. Guard with a flag so we only run on first activation.
            if (_activatedOnce) return;
            _activatedOnce = true;

            if (ViewModel.InitialiseDashboardCommand.CanExecute(null))
            {
                ViewModel.InitialiseDashboardCommand.Execute(null);
            }

            // Navigate to the dashboard page on startup
            ContentFrame.Navigate(typeof(Pages.DashboardPage), ViewModel);
            // select home in the nav menu
            RootNav.SelectedItem = HomeNavItem;
        };

        // No longer handle ViewModel.PropertyChanged in the window - the DashboardPage binds directly to the ViewModel

        // Keep frame navigation state in sync with the NavigationView
        ContentFrame.Navigated += ContentFrame_Navigated;
    }

    private void RootNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        // Determine which NavigationViewItem was invoked and navigate accordingly
        if (args.InvokedItemContainer is NavigationViewItem navItem)
        {
            switch (navItem.Name)
            {
                case nameof(HomeNavItem):
                    ContentFrame.Navigate(typeof(Pages.DashboardPage), ViewModel);
                    break;
            }
        }

        if (args.IsSettingsInvoked)
        {
            // Navigate to SettingsPage using Frame.Navigate so the back stack is maintained
            ContentFrame.Navigate(typeof(Pages.SettingsPage));
        }
    }

    private void RootNav_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        // Enable/disable the back button in the NavigationView
        RootNav.IsBackEnabled = ContentFrame.CanGoBack;

        // Keep selection in sync with current page
        if (e.SourcePageType == typeof(Pages.DashboardPage))
            RootNav.SelectedItem = HomeNavItem;
        else if (e.SourcePageType == typeof(Pages.SettingsPage))
            RootNav.SelectedItem = null; // keep settings unselected (it's a footer / settings action)
        else
            RootNav.SelectedItem = null;
    }

    // Backwards-compatible handler: some generated XAML may still reference BackButton_Click
    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }
}