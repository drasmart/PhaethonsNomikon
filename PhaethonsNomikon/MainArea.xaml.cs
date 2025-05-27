using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;

namespace PhaethonsNomikon;

public partial class MainArea : MyUserControl
{
    private readonly ObservableCollection<AgentListTabModel> _rawTabs = new();
    public ReadOnlyObservableCollection<AgentListTabModel> Tabs { get; }
    
    private readonly ObservableCollection<AgentData> _rawAgents = new();
    public ReadOnlyObservableCollection<AgentData> Agents { get; }
    
    public int SelectedTabIndex { get; set; }
    
    private const string AccountPage = "https://www.hoyolab.com/accountCenter";
    private const string ToLookFor1 = "getGameRecordCard";
    private const string ToLookFor2 = "https://act.hoyolab.com/app/zzz-game-record/[_a-zA-Z0-9?./=&-]+";
    private const string AgentBaseUrl = "https://act.hoyolab.com/app/zzz-game-record/#/zzz/roles/";
    private const string ListUrlFormatLog = AgentBaseUrl + "all?role_id={uid}&server={region}";
    private const string ListUrlFormatNav = AgentBaseUrl + "all?role_id={0}&server={1}";
    private const string ToLookFor3 = "avatar_list";
    private const string UserIdKey = "role_id";
    private const string ServerKey = "server";
    private const string AgentIdsKey = "id_list[]";
    private const string AgentUrlFormatLog = AgentBaseUrl + "{agent-id}/detail?" + UserIdKey + "={uid}&" + ServerKey + "={region}";
    private const string AgentUrlFormatNav = AgentBaseUrl + "{0}/detail?" + UserIdKey + "={1}&" + ServerKey + "={2}";
    private string? _uid, _region;
    private bool _isLoadingAgentList;
    
    public MainArea()
    {
        Tabs = new ReadOnlyObservableCollection<AgentListTabModel>(_rawTabs);
        Agents = new ReadOnlyObservableCollection<AgentData>(_rawAgents);
        InitializeComponent();
        OpenTab("Hoyolab Account", AccountPage, [ToLookFor1], ReadGameCard);
    }
    
    private void OnWebResourceResponseReceived(
        BrowserTabModel tab,
        CoreWebView2WebResourceResponseReceivedEventArgs e) 
    {
        var fullTab = (AgentListTabModel)tab;
        var request = e.Request;
        var response = e.Response;
        var uri = request.Uri;

        using var uriScope = Logger.BeginScope("{StatusCode}", response.StatusCode);
        Logger.LogDebug("HTTP Response\n{Uri}", uri);

        bool filtersPassed = true;
        foreach (var filter in fullTab.ResourceUrlFilters)
        {
            if (!uri.Contains(filter))
            {
                filtersPassed = false;
                break;
            }
        }
        if (filtersPassed)
        {
            CheckContent(uri, response, page => fullTab.OnResponseContent(fullTab, uri, page));
        }
    }
    
    private void ReadGameCard(AgentListTabModel tab, string uri, string page)
    {
        MatchCollection matches = Regex.Matches(page, ToLookFor2);
        if (matches.Count > 0)
        {
            foreach (Match match in matches)
            {
                Logger.LogInformation("Battle Record Link\n{src-Uri}\n{dst-Uri}", uri, match.Value);
            }
            NavigateToAgentList(page);
            CloseTab(tab);
        }
    }
    
    private void ReadAgentList(AgentListTabModel tab, string uri, string page)
    {
        if (page.Contains(ToLookFor3))
        {
            var agentRefs = HandleAgentsList(page);
            CloseTab(tab);
            if (_isLoadingAgentList && agentRefs.Count > 1)
            {
                _isLoadingAgentList = false;
                foreach (var agentRef in agentRefs)
                {
                    Logger.LogInformation("Agent Page URI\n" + AgentUrlFormatLog, agentRef.Id, _uid, _region);
                    var newUrl = string.Format(AgentUrlFormatNav, agentRef.Id, _uid, _region);
                    OpenTab(agentRef.Name, newUrl, [
                        $"{AgentIdsKey}={agentRef.Id}",
                        $"{UserIdKey}={_uid}",
                        $"{ServerKey}={_region}",
                    ], ReadAgentList);
                }
            }
        }
    }
    
    private async void CheckContent(
        string uri,
        CoreWebView2WebResourceResponseView response,
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
            Logger.LogError(ex, "HTTP Content Error: {StatusCode}", response.StatusCode);
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
        _isLoadingAgentList = true;
        OpenTab("Agents List", newUrl, [
            $"{UserIdKey}={_uid}",
            $"{ServerKey}={_region}",
        ], ReadAgentList);
    }
    
    private record AgentRef(int Id, string Name);

    private List<AgentRef> HandleAgentsList(string page)
    {
        Logger.LogInformation(nameof(HandleAgentsList));
        var doc = JsonDocument.Parse(page);
        var avList = doc.RootElement.GetProperty("data").GetProperty(ToLookFor3);
        var agentsCount = avList.GetArrayLength();
        Logger.LogInformation("Found {agent-count} agents", agentsCount);
        List<AgentRef> agentRefs = new();
        foreach (var avatar in avList.EnumerateArray())
        {
            var agentId = avatar.GetProperty("id").GetInt32();
            var agentName = avatar.GetProperty("full_name_mi18n").GetString();
            using (Logger.BeginScope("{agent-data}", avatar.ToString()))
                Logger.LogInformation("Agent ID {agent-id}: {agent-name}", agentId, agentName);
            agentRefs.Add(new AgentRef(agentId, agentName ?? $"Agent #{agentId}"));
            if (agentsCount == 1 && page.Contains("equip_plan_info"))
            {
                if (AgentData.FromJson(avatar, Logger) is { } agentData)
                {
                    _rawAgents.Add(agentData);
                }
            }
        }
        return agentRefs;
    }
    
    private static string HttpMessageContentToString(Stream content)
    {
        ArgumentNullException.ThrowIfNull(content);
        using var reader = new StreamReader(content, Encoding.UTF8, true, 1024, true);
        return reader.ReadToEnd();
    }

    private void OpenTab(
        string name,
        string url,
        IEnumerable<string> filters,
        Action<AgentListTabModel, string, string> onResponseContent)
    {
        _rawTabs.Add(new AgentListTabModel
        {
            Header = name,
            Url = url,
            OnResponseReceived = OnWebResourceResponseReceived,
            ResourceUrlFilters = filters,
            OnResponseContent = onResponseContent,
        });
        SelectedTabIndex = _rawTabs.Count - 1;
        MyTabControl.Visibility = Visibility.Visible;
        MainGrid.RowDefinitions[2].Height =  new GridLength(1, GridUnitType.Star);
    }

    private void CloseTab(BrowserTabModel tab)
    {
        _rawTabs.Remove((AgentListTabModel)tab);
        if (_rawTabs.Count == 0)
        {
            MyTabControl.Visibility = Visibility.Collapsed;
            MainGrid.RowDefinitions[2].Height =  new GridLength(0);
        }
    }

    public class AgentListTabModel : BrowserTabModel
    {
        public required IEnumerable<string> ResourceUrlFilters { get; init; }
        public required Action<AgentListTabModel, string, string> OnResponseContent { get; init; }
    }
}