using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PhaethonsNomikon.SaveData.v1;

[Serializable]
public class SaveContent : SaveHeader
{
    [Serializable]
    public class RawData
    {
        public JsonElement? GameCard { get; set; }
        public JsonElement? AgentList { get; set; }
        public List<JsonElement> AgentData { get; set; } = new();
    }

    public RawData RawResponses { get; set; } = new();
    
    public string? Uid { get; set; }
    public string? Server { get; set; }
    public List<AgentRef> AgentsList { get; set; } = new();
    public List<AgentData> AgentsData { get; set; } = new();
    
    

    public static SaveContent? ParseAgent(string agentJson, ILogger logger)
    {
        JsonDocument document = JsonDocument.Parse(agentJson);
        if (
            document.RootElement.TryGetProperty("data", out JsonElement data)
            && data.TryGetProperty("avatar_list", out JsonElement avatarList))
        {
            var result = new SaveContent();
            result.RawResponses.AgentData.Add(document.RootElement);
            for (int i = 0, n = avatarList.GetArrayLength(); i < n; i++)
            {
                if (AgentData.FromJson(avatarList[i], logger) is { } agent)
                {
                    result.AgentsData.Add(agent);
                }
            }
            return result;
        }
        return null;
    }
}