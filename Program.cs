using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.ApplicationModel.DynamicDependency;

namespace TB;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // ✅ SDK 2.1 requires 0x00020000 (v2.0.0.0)
        Bootstrap.Initialize(0x00020000);
        WinRT.ComWrappersSupport.InitializeComWrappers();
        
        Application.Start(p =>
        {
            var ctx = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(ctx);
            new App();
        });
    }
}
