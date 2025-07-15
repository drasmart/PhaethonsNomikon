using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PhaethonsNomikon.SaveData;

using Content = v1.SaveContent;

public class SaveHelper
{
    public static Content Load(string fileName, ILogger logger)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("File not found", fileName);
        }
        string json = File.ReadAllText(fileName);

        if (Content.ParseAgent(json, logger) is { } result)
        {
            return result;
        }
        
        var header = JsonSerializer.Deserialize<SaveHeader>(json) ?? throw new Exception("Invalid save file");
        switch (header.Version)
        {
            case 1:
                return JsonSerializer.Deserialize<Content>(json) ?? throw new Exception("Invalid save file");
            default:
                throw new Exception("Unsupported save file version");
        }
    }

    public static void Save(string fileName, Content content)
    {
        string json = JsonSerializer.Serialize(content, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, json);
    }
}
