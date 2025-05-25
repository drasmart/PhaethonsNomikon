using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;

namespace PhaethonsNomikon;

public partial class BrowserTab : UserControl
{   
    private readonly ILogger _logger;
    
    public BrowserTab()
    {
        _logger = ((App)Application.Current).ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        InitializeComponent();  
        InitializeWebView();
    }
    
    private async void InitializeWebView()
    {
        await myWebView.EnsureCoreWebView2Async();

        // Subscribe to WebResourceResponseReceived
        myWebView.CoreWebView2.WebResourceResponseReceived += WebView_WebResourceResponseReceived;

        // Navigate to a webpage
        myWebView.CoreWebView2.Navigate("https://www.hoyolab.com/accountCenter");
    }

    private void WebView_WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        using var _ = _logger.BeginScope("{function}", nameof(WebView_WebResourceResponseReceived));
        
        var request = e.Request;
        var response = e.Response;
        
        using (_logger.BeginScope("{Uri}", request.Uri))
        using (_logger.BeginScope("{StatusCode}", response.StatusCode))
        using (_logger.BeginScope("{Headers}", response.Headers))
            _logger.LogInformation("HTTP Response");
    }

}