using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace PhaethonsNomikon;

public abstract class MyUserControl : UserControl
{
    protected IServiceProvider Services { get; }
    protected ILogger Logger { get; }

    protected MyUserControl()
    {
        Services = ((App)Application.Current).ServiceProvider;
        Logger = Services.GetRequiredService<ILogger<MyUserControl>>();
    }
    
    protected void DispatchIfNecessary(Action action) {
        if (!Dispatcher.CheckAccess())
            Dispatcher.Invoke(action);
        else
            action.Invoke();
    }
    
    public static void SaveFullListViewAsImage(ListView listView, string fileName, string title)
    {
        SaveFileDialog saveFileDialog1 = new SaveFileDialog
        {
            FileName = fileName,
            Title = title,
            Filter = "PNG images (*.png)|*.png|All files (*.*)|*.*",
        };
        if (saveFileDialog1.ShowDialog() != true)
        {
            return;
        }
        
        // Store the original size
        double originalHeight = listView.ActualHeight;

        // Measure the entire ListView content
        listView.Measure(new Size(listView.ActualWidth, double.PositiveInfinity));
        listView.Arrange(new Rect(new Size(listView.ActualWidth, listView.DesiredSize.Height)));

        int width = (int)listView.ActualWidth;
        int height = (int)listView.DesiredSize.Height;

        var renderBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        renderBitmap.Render(listView);

        // Restore the original size
        listView.Measure(new Size(listView.ActualWidth, originalHeight));
        listView.Arrange(new Rect(new Size(listView.ActualWidth, originalHeight)));

        // Save the image
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

        using (var fileStream = new FileStream(saveFileDialog1.FileName, FileMode.Create))
        {
            encoder.Save(fileStream);
        }
    }
}