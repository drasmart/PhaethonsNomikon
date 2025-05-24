using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhaethonsNomikon;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly double _logBoxMinHeight;
    
    public MainWindow()
    {
        InitializeComponent();
        _logBoxMinHeight = LogContainer.RowDefinitions[2].MinHeight;
    }
    
    private void ToggleLogView_Checked(object sender, RoutedEventArgs e)
    {
        if (LogViewBox != null)
        {
            LogViewBox.Visibility = Visibility.Visible;
        }
        if (LogContainer?.RowDefinitions != null && LogContainer.RowDefinitions.Count >= 3)
        {
            LogContainer.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star); // Main content fills space
            LogContainer.RowDefinitions[1].Height = new GridLength(5); // Show splitter
            // Show log view
            LogContainer.RowDefinitions[2].MinHeight = _logBoxMinHeight;
            LogContainer.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
        }
    }

    private void ToggleLogView_Unchecked(object sender, RoutedEventArgs e)
    {
        if (LogViewBox != null)
        {
            LogViewBox.Visibility = Visibility.Collapsed;
        }
        if (LogContainer?.RowDefinitions != null && LogContainer.RowDefinitions.Count >= 3)
        {
            LogContainer.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star); // Main content fills space
            LogContainer.RowDefinitions[1].Height = new GridLength(0); // Hide splitter
            // Collapse log view
            LogContainer.RowDefinitions[2].MinHeight = 0;
            LogContainer.RowDefinitions[2].Height = new GridLength(0);
        }
    }
}