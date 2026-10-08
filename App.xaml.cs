using System;
using PaperPaint.Helpers;
using PaperPaint.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PaperPaint
{
    /// <summary>Provides application-specific behavior to supplement the default Application class.</summary>
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }
        public static Frame? ContentFrame { get; internal set; }
        public static IntPtr Hwnd { get; private set; }

        /// <summary>Initializes the singleton application object.</summary>
        public App()
        {
            InitializeComponent();
            UnhandledException += (sender, args) =>
            {
                AppLogger.LogException(args.Exception, "GlobalUnhandledException");
                args.Handled = true;
            };
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                MainWindow = new MainWindow();
                Hwnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
                MainWindow.Activate();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "OnLaunched");
            }
        }
    }
}
