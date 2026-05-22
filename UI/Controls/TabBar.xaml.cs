using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TB_Browser.Core.Services;

namespace TB_Browser.UI.Controls
{
    public sealed partial class TabBar : UserControl
    {
        private readonly ITabService _svc;
        public TabBar(ITabService svc) { InitializeComponent(); _svc = svc; BindTabs(); }

        private void BindTabs()
        {
            _svc.TabAdded += (_, t) => AddTabUI(t);
            _svc.TabRemoved += (_, t) => RemoveTabUI(t.Id);
            foreach (var t in _svc.Tabs) AddTabUI(t);
        }

        private void AddTabUI(TabModel t)
        {
            var btn = new Button { Content = t.Title, Style = (Style)Resources["TabBtn"] };
            btn.Click += (_, _) => _svc.ActivateTab(t.Id);
            btn.Tag = t.Id;
            TabsPanel.Children.Add(btn);
        }

        private void RemoveTabUI(int id)
        {
            foreach (Button b in TabsPanel.Children)
                if (b.Tag is int tid && tid == id) { TabsPanel.Children.Remove(b); break; }
        }

        private void Minimize_Click(object s, RoutedEventArgs e) => ((Window)XamlRoot.Content).Minimize();
        private void Maximize_Click(object s, RoutedEventArgs e) { var w = (Window)XamlRoot.Content; w.OverlappedPresenter.IsMaximized = !w.OverlappedPresenter.IsMaximized; }
        private void Close_Click(object s, RoutedEventArgs e) => Environment.Exit(0);
    }
}
