using System.Text.Json;

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
    
    public int Uid { get; set; }
    public string? Server { get; set; }
    public List<AgentRef> AgentsList { get; set; } = new();
    public List<AgentData> AgentsData { get; set; } = new();
}