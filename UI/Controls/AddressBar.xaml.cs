using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TB_Browser.Core.Logging;
using TB_Browser.Core.Services;

namespace TB_Browser.UI.Controls
{
    public partial class AddressBar : UserControl
    {
        private readonly IBrowserService _svc;
        public AddressBar(IBrowserService svc) { InitializeComponent(); _svc = svc; _svc.UrlChanged += (_, u) => UrlBox.Text = u; }
        private void Back_Click(object s, RoutedEventArgs e) { Logger.Debug("UI", "Back"); _svc.GoBack(); }
        private void Forward_Click(object s, RoutedEventArgs e) { Logger.Debug("UI", "Forward"); _svc.GoForward(); }
        private void Reload_Click(object s, RoutedEventArgs e) { Logger.Debug("UI", "Reload"); _svc.Reload(); }
        private void Go_Click(object s, RoutedEventArgs e) { Logger.Info("UI", $"Go: {UrlBox.Text}"); _svc.Navigate(UrlBox.Text); }
        private void Url_KeyDown(object s, KeyEventArgs e) { if (e.Key == Key.Enter) { Logger.Info("UI", $"Enter: {UrlBox.Text}"); _svc.Navigate(UrlBox.Text); } }
    }
}
