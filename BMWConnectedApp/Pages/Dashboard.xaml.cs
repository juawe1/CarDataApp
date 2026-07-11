using Bmw.Dashboard.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;

namespace BMWConnectedApp.Pages
{
    public sealed partial class DashboardPage : Page
    {
        public DashboardViewModel? ViewModel { get; private set; }

        public DashboardPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is DashboardViewModel vm)
            {
                ViewModel = vm;
                DataContext = ViewModel;
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
                SetHeroImage(ViewModel.VehicleImageSource);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (ViewModel != null)
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.VehicleImageSource))
                SetHeroImage(ViewModel!.VehicleImageSource);
        }

        private void SetHeroImage(string source)
        {
            if (string.IsNullOrEmpty(source)) return;
            HeroImage.Source = new BitmapImage(new Uri(source));
            HeroImage.Stretch = source.StartsWith("file://")
                ? Microsoft.UI.Xaml.Media.Stretch.Uniform
                : Microsoft.UI.Xaml.Media.Stretch.UniformToFill;
        }
    }
}
