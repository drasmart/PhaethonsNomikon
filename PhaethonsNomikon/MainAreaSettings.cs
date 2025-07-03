using System.ComponentModel;

namespace PhaethonsNomikon;

public class MainAreaSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isStripeVisible;

    public bool IsStripeVisible
    {
        get => _isStripeVisible;
        set
        {
            if (_isStripeVisible != value)
            {
                _isStripeVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsStripeVisible)));
            }
        }
    }
}
