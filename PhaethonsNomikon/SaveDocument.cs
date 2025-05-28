using System.IO;
using Microsoft.Win32;
using PhaethonsNomikon.SaveData;

namespace PhaethonsNomikon;

using Content = SaveData.v1.SaveContent;

[Serializable]
public class SaveDocument
{
    public string? FilePath { get; set; }
    public Content? Content { get; set; }
    public bool ShouldLoad { get; set; }
    public bool HasLoaded { get; set; }

    public SaveDocument(bool shouldLoad)
    {
        ShouldLoad = shouldLoad;
    }
    
    public SaveDocument(string filePath)
    {
        FilePath = filePath;
        Content = SaveHelper.Load(filePath);
    }

    public void Save()
    {
        if (Content is null || !HasLoaded)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save a Text File";
        
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
}
