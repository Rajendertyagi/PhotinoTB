using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using TB.Core.DI;
using TB.Data.Services;
using TB.Infrastructure;

namespace TB.Core;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
        InitializeServices();
    }

    private void InitializeServices()
    {
        var pathResolver = new PathResolver();
        pathResolver.EnsureDirectories();
        Services = Container.Register(pathResolver);
        Services.GetRequiredService<DbInitializer>().Initialize();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new MainWindow().Activate();
    }
}
