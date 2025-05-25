using Microsoft.Web.WebView2.Core;

namespace PhaethonsNomikon;

public class BrowserTabModel
{
    public required string Header { get; init; }
    public required string Url { get; init; }
    public required Action<BrowserTabModel, CoreWebView2WebResourceResponseReceivedEventArgs>? OnResponseReceived { get; init; }
}