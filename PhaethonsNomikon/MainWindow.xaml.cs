using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PhaethonsNomikon;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly double _logBoxMinHeight;
    private double _lastLogBoxHeight;
    private readonly ILogger _logger;
    
    public string AppTitle { get; }
        = "Phaethon's Nomikon " + Assembly.GetExecutingAssembly().GetName().Version;

    public SaveDocument Document { get; set; }
    
    public MainWindow()
    {
        _logger = ((App)Application.Current).ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        Document = SaveDocument.Open(_logger) ?? new(true);
        InitializeComponent();
        _logBoxMinHeight = LogContainer.RowDefinitions[2].MinHeight;
        ToggleLogView.IsChecked = false;
    }
    
    private void ToggleLogView_Checked(object sender, RoutedEventArgs e)
    {
        using (_logger.BeginScope("{show-logs}", true))
            _logger.LogInformation("Toggle Logs ON");
        if (LogViewBox != null)
        {
            LogViewBox.Visibility = Visibility.Visible;
        }
        if (LogContainer?.RowDefinitions != null && LogContainer.RowDefinitions.Count >= 3)
        {
            LogContainer.RowDefinitions[1].Height = new GridLength(5); // Show splitter
            // Show log view
            LogContainer.RowDefinitions[2].MinHeight = _logBoxMinHeight;
            LogContainer.RowDefinitions[2].Height = new GridLength(_lastLogBoxHeight);
        }
    }
    
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (LogViewBox != null && ToggleLogView.IsChecked == true)
        {
            MyUserControl.SaveFullListViewAsImage(LogViewBox.LogList, "logs.png", "Save logs as PNG");
        }
    }
    
    private void Button2_Click(object sender, RoutedEventArgs e)
    {
        FirstMainArea.SaveAgentList();
    }

    private void Button3_Click(object sender, RoutedEventArgs e)
    {
        FirstMainArea.SaveSelectedAgentList();
    }

    private void ToggleLogView_Unchecked(object sender, RoutedEventArgs e)
    {
        using (_logger.BeginScope("{show-logs}", false))
            _logger.LogInformation("Toggle Logs OFF");
        if (LogViewBox != null)
        {
            _lastLogBoxHeight = LogViewBox.ActualHeight;
            LogViewBox.Visibility = Visibility.Collapsed;
        }
        if (LogContainer?.RowDefinitions != null && LogContainer.RowDefinitions.Count >= 3)
        {
            LogContainer.RowDefinitions[1].Height = new GridLength(0); // Hide splitter
            // Collapse log view
            LogContainer.RowDefinitions[2].MinHeight = 0;
            LogContainer.RowDefinitions[2].Height = new GridLength(0);
        }
    }
}
