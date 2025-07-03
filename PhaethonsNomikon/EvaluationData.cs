using System.ComponentModel;
using System.Windows.Media;

namespace PhaethonsNomikon;

public class EvaluationData : INotifyPropertyChanged
{
    private string _evaluationLetter = "?";
    public string EvaluationLetter
    {
        get => _evaluationLetter;
        set
        {
            if (_evaluationLetter != value)
            {
                _evaluationLetter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EvaluationLetter)));
            }
        }
    }

    private Brush _evaluationColor = Brushes.Pink;
    public Brush EvaluationColor
    {
        get => _evaluationColor;
        set
        {
            if (_evaluationColor != value)
            {
                _evaluationColor = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EvaluationColor)));
            }
        }
    }

    private double _evaluationFontSize = 0;
    public double EvaluationFontSize
    {
        get => _evaluationFontSize;
        set
        {
            if (Math.Abs(_evaluationFontSize - value) > 0.01)
            {
                _evaluationFontSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EvaluationFontSize)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
