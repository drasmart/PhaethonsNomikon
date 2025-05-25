using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;

namespace PhaethonsNomikon;

public partial class BrowserTab : MyUserControl
{
    public static readonly DependencyProperty MyTextProperty =
        DependencyProperty.Register(nameof(Model), typeof(BrowserTabModel), typeof(BrowserTab), new PropertyMetadata(null));

    private bool _isReadyToLoad;
    
    public BrowserTabModel? Model
    {
        get => (BrowserTabModel)GetValue(MyTextProperty);
        set
        {
            SetValue(MyTextProperty, value);
            if (Model is not null)
            {
                lock (myWebView)
                {
                    if (_isReadyToLoad)
                    {
                        myWebView.CoreWebView2.Navigate(Model.Url);
                    }
                }
            }
        }
    }
    
    public BrowserTab()
    {
        InitializeComponent();  
        InitializeWebView();
    }
    
    private async void InitializeWebView()
    {
        await myWebView.EnsureCoreWebView2Async();

        // Subscribe to WebResourceResponseReceived
        myWebView.CoreWebView2.WebResourceResponseReceived += WebView_WebResourceResponseReceived;

        lock (myWebView)
        {
            _isReadyToLoad = true;
            // Navigate to a webpage
            if (Model is not null)
            {
                myWebView.CoreWebView2.Navigate(Model.Url);
            }
        }
    }

    private void WebView_WebResourceResponseReceived(
        object? sender,
        CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        Model?.OnResponseReceived?.Invoke(Model, e);
    }
}