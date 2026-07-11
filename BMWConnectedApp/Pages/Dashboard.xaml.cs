using Bmw.Dashboard.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;

namespace BMWConnectedApp.Pages
{
    public sealed partial class DashboardPage : Page
    {
        // Make this public so the XAML {x:Bind} can see it
        public DashboardViewModel ViewModel { get; private set; }

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is DashboardViewModel vm)
            {
                ViewModel = vm;
                this.DataContext = ViewModel; // sets {x:Bind} and traditional bindings
            }
        }
    }
}
