using System;
using System.IO;
using System.Reflection;
using PaperPaint.Helpers;
using PaperPaint.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace PaperPaint.Views
{
    /// <summary>Settings &amp; About: input guidance, permissions explanation, app details.</summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();

            try
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                VersionText.Text = v != null
                    ? $"Paper Paint — {v.Major}.{v.Minor}.{v.Build}"
                    : "Paper Paint";
            }
            catch (Exception)
            {
                VersionText.Text = "Paper Paint";
            }
        }

        private async void OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = await ArtworkStore.GetRootAsync();
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "SettingsPage.OpenDataFolder");
            }
        }
    }
}
