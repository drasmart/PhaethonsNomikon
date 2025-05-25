using System.Text.Json;

namespace PhaethonsNomikon;

public class AgentData
{
    public required string Name { get; init; }
    public required string FullName { get; init; }
    public required string IconUrl { get; init; }
    public required int Level { get; init; }
    public required int Rank { get; init; }
    public required string Rarity { get; init; }
    public required IReadOnlyList<AgentProperty> AgentStats { get; init; }
    public required IReadOnlyList<PreferredProperty> PreferredStats { get; init; }
    public required Item? Weapon { get; init; }
    public required IReadOnlyList<Item?> Equipment { get; init; }

    public class ItemProperty
    {
        public required string Name { get; init; }
        public required string Base { get; init; }
        public required int Level { get; init; }
        public required int Add { get; init; }
    }

    public class AgentProperty
    {
        public required string Name { get; init; }
        public required string Base { get; init; }
        public required string Add { get; init; }
        public required string Final { get; init; }
    }

    public class PreferredProperty
    {
        public required string Name { get; init; }
        public required string FullName { get; init; }
    }

    public class Item
    {
        public required string Name { get; init; }
        public required string IconUrl { get; init; }
        public required int Level { get; init; }
        public int? Star { get; init; }
        public required string Rarity { get; init; }
        public required IReadOnlyList<ItemProperty> Stats { get; init; }
    }

    public static AgentData FromJson(JsonElement json)
    {
        return new AgentData
        {
            Name = json.GetProperty("name_mi18n").GetString()!,
            FullName = json.GetProperty("full_name_mi18n").GetString()!,
            IconUrl = json.GetProperty("role_square_url").GetString()!,
            Level = json.GetProperty("level").GetInt32(),
            Rank = json.GetProperty("rank").GetInt32(),
            Rarity = json.GetProperty("rarity").GetString()!,
        };
    }
}
