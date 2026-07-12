using Bmw.Dashboard.Core.Data.Entities;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Microsoft.UI.Xaml;
using System;
using Bmw.Dashboard.Core.ViewModels;

namespace BMWConnectedApp.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public UserConfigEntity? UserConfig { get; private set; }

        private SettingsViewModel? _viewModel;
        private ContentDialog? _verificationDialog;

        public SettingsPage() => this.InitializeComponent();

        protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Resolve the DI-resolved SettingsViewModel and bind it
            _viewModel = App.GetService<SettingsViewModel>();
            if (_viewModel != null)
            {
                this.DataContext = _viewModel;
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                await _viewModel.LoadAsync();
            }

            // Wire simple button handlers to call into the ViewModel commands/methods
            SaveButton.Click += async (s, ev) =>
            {
                if (_viewModel != null)
                {
                    await _viewModel.SaveAsync();
                    SaveStatus.Text = _viewModel.StatusText;
                }
            };

            AuthorizeButton.Click += (s, ev) =>
            {
                if (_viewModel != null)
                {
                    // Start authorization flow in background; UI will react to ViewModel state changes
                    _ = _viewModel.AuthorizeAsync();
                }
                else
                {
                    SaveStatus.Text = "ViewModel unavailable";
                }
            };

            QueryContainersButton.Click += async (s, ev) =>
            {
                if (_viewModel != null)
                {
                    ContainersList.ItemsSource = await _viewModel.QueryContainersAsync();
                }
            };
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not SettingsViewModel vm) return;

            // When device verification URI is set and polling has started, show the verification dialog
            if (e.PropertyName == nameof(vm.DeviceVerificationUri) || e.PropertyName == nameof(vm.IsPolling))
            {
                if (!string.IsNullOrEmpty(vm.DeviceVerificationUri) && vm.IsPolling)
                {
                    // Ensure UI work runs on dispatcher queue
                    this.DispatcherQueue.TryEnqueue(async () =>
                    {
                        // Create dialog and show it while ViewModel polls for token
                        _verificationDialog = CreateVerificationDialog(vm.DeviceVerificationUri, vm.UserCode, out var statusText);
                        _verificationDialog.XamlRoot = this.XamlRoot;
                        await _verificationDialog.ShowAsync();

                        // After dialog closes, update save status from ViewModel
                        SaveStatus.Text = vm.StatusText;
                    });
                }
                else if (!vm.IsPolling && _verificationDialog != null)
                {
                    // Close dialog when polling ends
                    this.DispatcherQueue.TryEnqueue(() => _verificationDialog?.Hide());
                }
            }
        }

        private void VerificationLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hb && hb.Tag is string uri)
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo(uri) { UseShellExecute = true };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to open verification link: {ex.Message}");
                }
            }
        }

        private ContentDialog CreateVerificationDialog(string verificationUri, string userCode, out TextBlock statusText)
        {
            statusText = new TextBlock
            {
                Text = "Waiting for authorization...",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray),
                TextWrapping = TextWrapping.Wrap
            };

            var dlg = new ContentDialog
            {
                Title = "Authorize Application",
                PrimaryButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary,
                Content = new StackPanel
                {
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "1) Open the verification link below in your browser.", TextWrapping = TextWrapping.Wrap, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray) },
                        new HyperlinkButton { Content = verificationUri, Tag = verificationUri, HorizontalAlignment = HorizontalAlignment.Left },
                        new TextBlock { Text = "2) Enter the code below when prompted:", TextWrapping = TextWrapping.Wrap, Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Gray) },
                        new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            Spacing = 8,
                            Children =
                            {
                                new StackPanel
                                {
                                    Orientation = Orientation.Horizontal,
                                    Spacing = 6,
                                    Children =
                                    {
                                        new TextBlock { Text = userCode, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.Bold, MinWidth = 160 },
                                        new Button { Content = "Copy", HorizontalAlignment = HorizontalAlignment.Left }
                                    }
                                },
                                new ProgressRing { IsActive = true, Width = 36, Height = 36 }
                            }
                        },
                        statusText
                    }
                }
            };

            // In WinUI3 desktop apps the dialog needs a XamlRoot tied to the current page/window
            dlg.XamlRoot = this.XamlRoot;

            // Attach click handlers for link and copy button
            if (dlg.Content is StackPanel sp && sp.Children.OfType<HyperlinkButton>().FirstOrDefault() is HyperlinkButton hb)
            {
                hb.Click += VerificationLink_Click;
            }

            // copy button
            if (dlg.Content is StackPanel sp2)
            {
                var inner = sp2.Children.OfType<StackPanel>().LastOrDefault();
                if (inner != null)
                {
                    var container = inner.Children.OfType<StackPanel>().FirstOrDefault();
                    if (container != null)
                    {
                        if (container.Children.OfType<Button>().FirstOrDefault() is Button copyBtn)
                        {
                            copyBtn.Click += (s, e) =>
                            {
                                try
                                {
                                    var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                                    dp.SetText(userCode ?? string.Empty);
                                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Copy in dialog failed: {ex.Message}");
                                }
                            };
                        }
                    }
                }
            }

            return dlg;
        }
    }
}
