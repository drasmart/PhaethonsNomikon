using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace PhaethonsNomikon;

[Serializable]
public class AgentData
{
    [JsonPropertyName("name_mi18n")]
    public string? Name { get; set; }
    [JsonPropertyName("full_name_mi18n")]
    public string? FullName { get; set; }
    [JsonPropertyName("us_full_name")]
    public string? FullNameUs { get; set; }
    [JsonPropertyName("role_square_url")]
    public string? IconUrl { get; set; }
    public int Level { get; set; }
    public int Rank { get; set; }
    public string? Rarity { get; set; }
    [JsonPropertyName("properties")]
    public List<AgentProperty>? AgentStats { get; set; }
    // "equip_plan_info.game_default.property_list"
    public List<PreferredProperty>? PreferredStats { get; set; }
    public Item? Weapon { get; set; }
    [JsonPropertyName("equip")]
    public List<Item?>? Equipment { get; set; }

    [Serializable]
    public class ItemProperty
    {
        [JsonPropertyName("property_name")]
        public string? Name { get; set; }
        public string? Base { get; set; }
        public int? Level { get; set; }
        public int? Add { get; set; }

        public override string ToString()
            => Add is {} add && add > 0 
                ? $"{Name} (+{add}) {Base}" 
                : $"{Name} {Base}";
    }

    [Serializable]
    public class AgentProperty
    {
        [JsonPropertyName("property_name")]
        public string? Name { get; set; }
        public string? Base { get; set; }
        public string? Add { get; set; }
        public string? Final { get; set; }

        public override string ToString() 
            => !string.IsNullOrEmpty(Base) && !string.IsNullOrEmpty(Add) 
                ? $"{Name} {Final} ({Base} + {Add})"
                : $"{Name} {Final}";
    }

    [Serializable]
    public class PreferredProperty
    {
        public string? Name { get; set; }
        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        public override string ToString() => Name ?? FullName ?? nameof(PreferredProperty);
    }

    [Serializable]
    public class Item
    {
        public string? Name { get; set; }
        [JsonPropertyName("icon")]
        public string? IconUrl { get; set; }
        public int Level { get; set; }
        public int? Star { get; set; }
        public string? Rarity { get; set; }
        [JsonPropertyName("main_properties")]
        public List<ItemProperty>? MainProperties { get; set; }
        public List<ItemProperty>? Properties { get; set; }
        public IEnumerable<ItemProperty> AllProperties => (MainProperties ?? []).Concat(Properties ?? []);

        public override string ToString()
            => (Star is { } star)
                ? $"{Name} P{star} ({Rarity}) lvl.{Level}"
                : $"{Name} ({Rarity}) lvl.{Level}";
    }

    public static AgentData? FromJson(JsonElement json, ILogger? logger)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var result = json.Deserialize<AgentData>(options);
        if (result is not null
            && json.TryGetProperty("equip_plan_info", out var equipPlanInfo) 
            && equipPlanInfo.ValueKind == JsonValueKind.Object
            && equipPlanInfo.TryGetProperty("game_default", out var gameDefaults)
            && gameDefaults.ValueKind == JsonValueKind.Object
            && gameDefaults.TryGetProperty("property_list", out var propertyList)
            && propertyList.ValueKind == JsonValueKind.Array)
        {
            result.PreferredStats = propertyList.Deserialize<List<PreferredProperty>>(options);
        }
        else if (logger is not null)
        {
            using (logger.BeginScope("{json}", json))
                logger.LogWarning("Failed to get preferred properties for {agent-name}", result?.FullNameUs);
        }
        return result;
    }

    public override string ToString() => $"{Name} M{Rank} ({Rarity}) lvl.{Level}";
}
