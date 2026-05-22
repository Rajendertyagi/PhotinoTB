using Microsoft.UI.Xaml;
namespace TB_Browser;
public partial class App : Application
{
    public App() => InitializeComponent();
    protected override void OnLaunched(LaunchActivatedEventArgs args) => new MainWindow().Activate();
}
