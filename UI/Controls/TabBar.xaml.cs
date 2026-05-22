using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // ✅ Added
using System.Windows.Media;
using TB_Browser.Core.Logging;
using TB_Browser.Core.Services;
using TB_Browser.Core.Models;

namespace TB_Browser.UI.Controls
{
    public partial class TabBar : UserControl
    {
        private readonly ITabService _svc;

        public TabBar(ITabService svc)
        {
            InitializeComponent();
            _svc = svc;
            _svc.TabAdded += (_, t) => AddTabUI(t);
            _svc.TabRemoved += (_, t) => RemoveTabUI(t.Id);
            Logger.Info("TabBar", "Control initialized");
        }

        private void AddTabUI(TabModel t)
        {
            Logger.Info("TabBar", $"Adding tab #{t.Id}");
            var grid = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,4,0) };
            
            var btn = new Button 
            { 
                Content = t.Title, MinWidth = 80, MaxWidth = 240, Height = 34,
                Background = (Brush)FindResource("ChromeTabInactiveBg"),
                Foreground = (Brush)FindResource("ChromeDisabled"),
                BorderThickness = new Thickness(0), Padding = new Thickness(10,0,0,0),
                Tag = t.Id, Cursor = Cursors.Hand // ✅ Now works
            };
            
            var close = new Button 
            { 
                Content = "✕", Width = 16, Height = 16, Background = Brushes.Transparent, 
                BorderThickness = new Thickness(0), Foreground = (Brush)FindResource("ChromeDisabled"), 
                Margin = new Thickness(6,0,4,0), FontSize = 10, Cursor = Cursors.Hand // ✅ Now works
            };
            
            btn.Click += (_, _) => { Logger.Info("TabBar", $"Tab #{t.Id} clicked"); Activate(t.Id); };
            close.Click += (_, _) => { Logger.Info("TabBar", $"Close tab #{t.Id} clicked"); _svc.CloseTab(t.Id); };
            
            grid.Children.Add(btn); grid.Children.Add(close);
            TabsPanel.Children.Add(grid);
            Activate(t.Id);
        }
        
        private void Activate(int id)
        {
            Logger.Info("TabBar", $"Activating tab #{id}");
            foreach (StackPanel sp in TabsPanel.Children)
            {
                var btn = (Button)sp.Children[0];
                bool isActive = (int)btn.Tag == id;
                btn.Background = isActive ? (Brush)FindResource("ChromeTabActiveBg") : (Brush)FindResource("ChromeTabInactiveBg");
                btn.Foreground = isActive ? (Brush)FindResource("ChromeText") : (Brush)FindResource("ChromeDisabled");
            }
            _svc.ActivateTab(id);
        }
        
        private void RemoveTabUI(int id)
        {
            Logger.Info("TabBar", $"Removing tab #{id}");
            foreach (StackPanel sp in TabsPanel.Children)
            {
                if ((int)((Button)sp.Children[0]).Tag == id) { TabsPanel.Children.Remove(sp); break; }
            }
        }
        
        private void NewTab_Click(object s, RoutedEventArgs e) { Logger.Info("TabBar", "New Tab (+) clicked"); _svc.CreateTab(); }
        public void CreateNewTab() => _svc.CreateTab();
        public void CloseActive() => _svc.CloseTab(_svc.ActiveTab?.Id ?? 0);
        private void Minimize_Click(object s, RoutedEventArgs e) => Window.GetWindow(this)?.SetWindowState(WindowState.Minimized);
        private void Maximize_Click(object s, RoutedEventArgs e) 
        { 
            var w = Window.GetWindow(this); 
            if (w != null) w.WindowState = w.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized; 
        }
        private void Close_Click(object s, RoutedEventArgs e) { Logger.Info("TabBar", "Close clicked"); Application.Current.Shutdown(); }
    }
}
