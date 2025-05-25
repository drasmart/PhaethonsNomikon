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
    private const string AccountPage = "https://www.hoyolab.com/accountCenter";
    private const string ToLookFor1 = "getGameRecordCard";
    private const string ToLookFor2 = "https://act.hoyolab.com/app/zzz-game-record/[_a-zA-Z0-9?./=&-]+";
    private const string AgentBaseUrl = "https://act.hoyolab.com/app/zzz-game-record/#/zzz/roles/";
    private const string ListUrlFormatLog = AgentBaseUrl + "all?role_id={uid}&server={region}";
    private const string ListUrlFormatNav = AgentBaseUrl + "all?role_id={0}&server={1}";
    private const string ToLookFor3 = "avatar_list";
    private const string AgentUrlFormatLog = AgentBaseUrl + "{agent-id}/detail?role_id={uid}&server={region}";
    private const string AgentUrlFormatNav = AgentBaseUrl + "{0}/detail?role_id={1}&server={2}";
    private string? _uid, _region;
    
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

        // Navigate to a webpage
        myWebView.CoreWebView2.Navigate(AccountPage);
    }

    private void WebView_WebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        var request = e.Request;
        var response = e.Response;
        var uri = request.Uri;

        using var uriScope = Logger.BeginScope("{StatusCode}", response.StatusCode);
        Logger.LogDebug("HTTP Response\n{Uri}", uri);

        if (uri.Contains(ToLookFor1))
        {
            CheckContent(uri, response, page =>
            {
                MatchCollection matches = Regex.Matches(page, ToLookFor2);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        Logger.LogInformation("Battle Record Link\n{src-Uri}\n{dst-Uri}", uri, match.Value);
                    }
                    NavigateToAgentList(page);
                }
            });
        }
        else if (_uid is { } uid
                 && _region is { } region
                 && uri.Contains(uid)
                 && uri.Contains(region))
        {
            CheckContent(uri, response, page =>
            {
                if (page.Contains(ToLookFor3))
                {
                    HandleAgentsList(page);
                }
            });
        }
    }

    private async void CheckContent(
        string uri,
        CoreWebView2WebResourceResponseView? response,
        Action<string> onSuccess)
    {
        try
        {
            Stream content = await response.GetContentAsync();
            DispatchIfNecessary(() =>
            {
                var page = HttpMessageContentToString(content);
                using (Logger.BeginScope("{http-content}", page))
                    Logger.LogInformation("HTTP Content\n{Uri}", uri);
                onSuccess(page);
            });
        }
        catch (System.Runtime.InteropServices.COMException ex)
        {
            Logger.LogError(ex, "HTTP Content Error: {StatusCode}", response?.StatusCode);
        }
    }

    private void NavigateToAgentList(string page)
    {
        var doc = JsonDocument.Parse(page);
        var list = doc.RootElement
            .GetProperty("data")
            .GetProperty("list");
        var gameCard = list.EnumerateArray()
            .First(x => x.GetProperty("game_id").GetInt32() == 8);
        _uid = gameCard.GetProperty("game_role_id").ToString();
        _region = gameCard.GetProperty("region").ToString();
        Logger.LogInformation("Agent Page URI\n" + ListUrlFormatLog, _uid, _region);
        var newUrl = string.Format(ListUrlFormatNav, _uid, _region);
        DispatchIfNecessary(() => myWebView.CoreWebView2.Navigate(newUrl));
    }

    private void HandleAgentsList(string page)
    {
        Logger.LogInformation(nameof(HandleAgentsList));
        var doc = JsonDocument.Parse(page);
        var avList = doc.RootElement.GetProperty("data").GetProperty(ToLookFor3);
        Logger.LogInformation("Found {agent-count} agents", avList.GetArrayLength());
        List<int> agentIds = new();
        foreach (var avatar in avList.EnumerateArray())
        {
            var agentId = avatar.GetProperty("id").GetInt32();
            using (Logger.BeginScope("{agent-data}", avatar.ToString()))
                Logger.LogInformation("Agent ID {agent-id}: {agent-name}",
                    agentId,
                    avatar.GetProperty("full_name_mi18n").GetString());
            agentIds.Add(agentId);
        }
        if (agentIds.Count > 1)
        {
            Logger.LogInformation("Agent Page URI\n" + AgentUrlFormatLog, agentIds[0], _uid, _region);
            var newUrl = string.Format(AgentUrlFormatNav, agentIds[0], _uid, _region);
            DispatchIfNecessary(() => myWebView.CoreWebView2.Navigate(newUrl));
        }
    }
    
    private static string HttpMessageContentToString(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);
        using var reader = new StreamReader(content, Encoding.UTF8, true, 1024, true);
        return reader.ReadToEnd();
    }

}