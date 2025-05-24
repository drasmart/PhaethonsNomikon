using System.Configuration;
using System.Data;
using System.Transactions;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySimpleLogging;

namespace PhaethonsNomikon;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        base.OnStartup(e);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        LoggingService listLogger = new(() => DateTime.Now);
        services.AddSingleton(listLogger);
        
        // Configure Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddProvider(listLogger);
        });

        // // Register Services
        // services.AddSingleton<IUserService, UserService>();
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        // Dispose of services if needed
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

