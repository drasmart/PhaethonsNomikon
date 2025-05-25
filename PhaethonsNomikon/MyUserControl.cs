using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PhaethonsNomikon;

public abstract class MyUserControl : UserControl
{
    protected IServiceProvider Services { get; }
    protected ILogger Logger { get; }

    protected MyUserControl()
    {
        Services = ((App)Application.Current).ServiceProvider;
        Logger = Services.GetRequiredService<ILogger<MyUserControl>>();
    }
    
    protected void DispatchIfNecessary(Action action) {
        if (!Dispatcher.CheckAccess())
            Dispatcher.Invoke(action);
        else
            action.Invoke();
    }
}