using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ScreenBrightnessControl
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Configure window size and prevent resizing using AppWindow
            TrySetWindowSize(500, 750);
        }

        private void TrySetWindowSize(int width, int height)
        {
            try
            {
                nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                AppWindow? appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                if (appWindow is not null)
                {
                    appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });
                    MoveToCenter(appWindow, windowId, width, height);
                }
            }
            catch
            {

            }
        }

        private void MoveToCenter(AppWindow appWindow, WindowId windowId, int width, int height)
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            RectInt32 work = displayArea.WorkArea;
            int centerX = work.X + (work.Width - width) / 2;
            int centerY = work.Y + (work.Height - height) / 2;
            appWindow.Move(new Windows.Graphics.PointInt32 { X = centerX, Y = centerY });
        }

        private void SldLaptopBrtn_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            ScreenBrightnessControl.SetLaptopScreenBrightness(((int)e.NewValue));
        }

        private void SldMtrBrtn_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            ScreenBrightnessControl.SetMonitorBrightness(((int)e.NewValue));
        }
    }
}
