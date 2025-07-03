using System.ComponentModel;

namespace PhaethonsNomikon;

public class MainAreaSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _treatFallbackDistinctly;

    public bool TreatFallbackDistinctly
    {
        get => _treatFallbackDistinctly;
        set
        {
            if (_treatFallbackDistinctly != value)
            {
                _treatFallbackDistinctly = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TreatFallbackDistinctly)));
            }
        }
    }
}
