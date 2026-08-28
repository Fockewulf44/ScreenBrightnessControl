using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using ScreenBrightnessControl.ViewModels;
using Windows.Graphics;

namespace ScreenBrightnessControl
{
    /// <summary>
    /// Hosts the brightness sliders. The window is responsible for presentation only;
    /// all brightness behaviour lives in <see cref="MainViewModel"/>.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private const int WindowWidth = 500;
        private const int WindowHeight = 750;

        public MainWindow(MainViewModel viewModel)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            // Assigned before InitializeComponent so the compiled bindings resolve on first pass.
            InitializeComponent();

            TrySetWindowSize(WindowWidth, WindowHeight);

            Activated += OnFirstActivated;
            Closed += OnClosed;
        }

        /// <summary>
        /// Bound by the compiled bindings in MainWindow.xaml.
        /// </summary>
        public MainViewModel ViewModel { get; }

        private async void OnFirstActivated(object sender, WindowActivatedEventArgs args)
        {
            Activated -= OnFirstActivated;

            try
            {
                // Reads the brightness the displays actually report so the sliders open in sync.
                await ViewModel.LoadAsync();
            }
            catch (Exception exception)
            {
                // This handler is async void, so an escaping exception would tear down the process.
                Debug.WriteLine($"The displays could not be read on start-up: {exception}");
            }
        }

        private void OnClosed(object sender, WindowEventArgs args)
        {
            Closed -= OnClosed;
            ViewModel.Dispose();
        }

        private void TrySetWindowSize(int width, int height)
        {
            AppWindow? appWindow = TryGetAppWindow();

            if (appWindow is null)
            {
                return;
            }

            appWindow.Resize(new SizeInt32 { Width = width, Height = height });
            MoveToCenter(appWindow, width, height);
        }

        private AppWindow? TryGetAppWindow()
        {
            try
            {
                nint windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
                return AppWindow.GetFromWindowId(windowId);
            }
            catch (Exception exception) when (exception is COMException or ArgumentException)
            {
                Debug.WriteLine($"The window could not be positioned: {exception.Message}");
                return null;
            }
        }

        private static void MoveToCenter(AppWindow appWindow, int width, int height)
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Nearest);
            RectInt32 work = displayArea.WorkArea;

            appWindow.Move(new PointInt32
            {
                X = work.X + ((work.Width - width) / 2),
                Y = work.Y + ((work.Height - height) / 2),
            });
        }
    }
}
