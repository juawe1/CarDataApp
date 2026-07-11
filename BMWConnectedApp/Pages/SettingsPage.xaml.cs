using Bmw.Dashboard.Core.Data.DbContexts;
using Bmw.Dashboard.Core.Data.Entities;
using Bmw.Dashboard.Core.Interfaces;
using Bmw.Dashboard.Core.Models.API;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Microsoft.UI.Xaml;
using System;

namespace BMWConnectedApp.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public UserConfigEntity UserConfig { get; private set; }

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Begin device code flow using the BmwApiService (PKCE)
            try
            {
                var api = App.GetService<Bmw.Dashboard.Core.Interfaces.IBmwApiService>();
                if (api == null)
                {
                    SaveStatus.Text = "API service unavailable";
                    return;
                }

                // Generate a code verifier and challenge (PKCE S256)
                var codeVerifier = GenerateCodeVerifier();
                var codeChallenge = ComputeCodeChallenge(codeVerifier);

                var deviceCodeResp = await api.GetDeviceCodeAsync(codeChallenge);
                if (deviceCodeResp == null)
                {
                    SaveStatus.Text = "Failed to start authorization";
                    return;
                }

                // Create a modal dialog and start background polling; dialog will close automatically when token is retrieved
                var dialog = CreateVerificationDialog(deviceCodeResp.VerificationUri, deviceCodeResp.UserCode, out var statusText);

                // Start polling for token in background
                var tokenTask = api.PollForTokenAsync(deviceCodeResp.DeviceCode, codeVerifier, deviceCodeResp.Interval);

                var pv = App.GetService<Bmw.Dashboard.Core.Interfaces.IPasswordVaultService>();

                _ = tokenTask.ContinueWith(t =>
                {
                    try
                    {
                        if (t.Status == System.Threading.Tasks.TaskStatus.RanToCompletion && t.Result != null)
                        {
                            // Save refresh token
                            try { pv?.SaveTokens(t.Result.RefreshToken ?? string.Empty); } catch { }

                            // Update UI and close dialog on UI thread
                            try
                            {
                                dialog.DispatcherQueue.TryEnqueue(() =>
                                {
                                    try
                                    {
                                        statusText.Text = "Authorized";
                                        dialog.Hide();
                                    }
                                    catch { }
                                });
                            }
                            catch { }
                        }
                        else
                        {
                            // Polling finished without token (timeout or error) - update status
                            try
                            {
                                dialog.DispatcherQueue.TryEnqueue(() => { try { statusText.Text = "Authorization not completed"; } catch { } });
                            }
                            catch { }
                        }
                    }
                    catch { }
                });

                // Show the dialog modally. It will remain open while polling runs in background.
                await dialog.ShowAsync();

                // After dialog closes, check token result
                TokenResponse? token = null;
                try
                {
                    token = tokenTask.Status == System.Threading.Tasks.TaskStatus.RanToCompletion ? tokenTask.Result : null;
                }
                catch { }

                if (token == null)
                {
                    SaveStatus.Text = "Authorization not completed";
                    return;
                }

                SaveStatus.Text = "Authorized";
            }
            catch (Exception ex)
            {
                SaveStatus.Text = "Authorization failed";
                System.Diagnostics.Debug.WriteLine($"Authorize error: {ex.Message}");
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

        // Inline copy handler removed; copy is handled inside the dialog.

        private static string GenerateCodeVerifier()
        {
            var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Base64UrlEncode(bytes);
        }

        private static string ComputeCodeChallenge(string codeVerifier)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.ASCII.GetBytes(codeVerifier);
            var hash = sha256.ComputeHash(bytes);
            return Base64UrlEncode(hash);
        }

        private static string Base64UrlEncode(byte[] arg)
        {
            var s = Convert.ToBase64String(arg);
            s = s.Split('=')[0]; // Remove any trailing '='s
            s = s.Replace('+', '-'); // 62nd char of encoding
            s = s.Replace('/', '_'); // 63rd char of encoding
            return s;
        }

        protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Use the SettingsService to populate the page so the service maintains a cache
            var settingsService = App.GetService<ISettingsService>();
            await settingsService.LoadAsync();
            UserConfig = settingsService.Current ?? new UserConfigEntity();
            this.DataContext = this;
            // Enable the authorize button only if there is a client id
            AuthorizeButton.IsEnabled = !string.IsNullOrWhiteSpace(UserConfig.ClientId);
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsService = App.GetService<ISettingsService>();
                await settingsService.SaveAsync(UserConfig);
                SaveStatus.Text = "Saved";
            }
            catch (Exception ex)
            {
                SaveStatus.Text = "Save failed";
                System.Diagnostics.Debug.WriteLine($"Save error: {ex.Message}");
            }
        }
    }
}
