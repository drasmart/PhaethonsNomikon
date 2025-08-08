using System.IO;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using PhaethonsNomikon.SaveData;

namespace PhaethonsNomikon;

using Content = SaveData.v1.SaveContent;

[Serializable]
public class SaveDocument
{
    public static string DefaultSaveName => $"agents_{DateTime.Now:yyMMdd_HHmmss}";
    
    public string? FilePath { get; set; }
    public Content Content { get; set; } = new();
    public bool ShouldLoad { get; set; }
    public bool HasLoaded { get; set; }

    public SaveDocument(bool shouldLoad)
    {
        ShouldLoad = shouldLoad;
    }
    
    public SaveDocument(string filePath, ILogger logger)
    {
        FilePath = filePath;
        Content = SaveHelper.Load(filePath, logger);
        HasLoaded = true;
    }
    
    private const string Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
    private const string Title = "Agents Json File";

    public void Save()
    {
        if (!HasLoaded)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog
            {
                Filter = Filter,
                Title = "Save " + Title,
                FileName = !string.IsNullOrWhiteSpace(FilePath)
                    ? Path.GetFileNameWithoutExtension(FilePath)
                    : DefaultSaveName,
            };
            if (saveFileDialog1.ShowDialog() == true)
            {
                FilePath = saveFileDialog1.FileName;
            }
        }
        if (!string.IsNullOrWhiteSpace(FilePath))
        {
            SaveHelper.Save(FilePath, Content);
        }
    }
    
    public static SaveDocument? Open(ILogger logger)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            Filter = Filter,
            Title = "Open " + Title,
        };
        return openFileDialog.ShowDialog() == true
            ? new SaveDocument(openFileDialog.FileName, logger)
            : null;
    }
}
