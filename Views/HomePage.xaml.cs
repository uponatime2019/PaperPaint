using System;
using System.Linq;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace PaperPaint.Views
{
    /// <summary>Home / gallery: entry points for new paintings, photo import, camera, coloring pages and saved artwork.</summary>
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _ = LoadRecentAsync();
        }

        private void NewPainting_Click(object sender, RoutedEventArgs e)
        {
            Nav.Go<StudioPage>(new StudioLaunch());
        }

        private void ColoringPages_Click(object sender, RoutedEventArgs e) => Nav.Go<ColoringGalleryPage>();

        private void MyArtwork_Click(object sender, RoutedEventArgs e) => Nav.Go<ArtworkPage>();

        private async void ImportPhoto_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.Hwnd);
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }

            try
            {
                using var stream = await file.OpenReadAsync();
                var payload = await ImageOps.DecodeFittedAsync(stream, System.IO.Path.GetFileNameWithoutExtension(file.Name));
                Nav.Go<EffectsPage>(payload);
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "HomePage.ImportPhoto");
            }
        }

        private async void Camera_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CameraDialog { XamlRoot = XamlRoot };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.Result != null)
            {
                Nav.Go<EffectsPage>(dialog.Result);
            }
        }

        // ------------------------------------------------------------------
        // recent artwork strip
        // ------------------------------------------------------------------

        private async Task LoadRecentAsync()
        {
            try
            {
                var items = await ArtworkStore.LoadIndexAsync();
                var recent = items
                    .OrderByDescending(x => x.ModifiedUtc)
                    .Take(6)
                    .ToList();

                RecentList.Children.Clear();
                RecentSection.Visibility = recent.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

                foreach (var item in recent)
                {
                    var thumb = new BitmapImage();
                    var stream = await ArtworkStore.OpenThumbnailAsync(item);
                    if (stream != null)
                    {
                        await thumb.SetSourceAsync(stream);
                    }

                    var card = new Button
                    {
                        Tag = item,
                        Padding = new Thickness(0),
                        CornerRadius = new CornerRadius(10),
                        Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)),
                        BorderBrush = Application.Current.Resources["LineBrush"] as Microsoft.UI.Xaml.Media.SolidColorBrush,
                        BorderThickness = new Thickness(1),
                        MinWidth = 168,
                        MaxWidth = 220,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    AutomationProperties.SetName(card, $"Open {item.Name}");

                    var image = new Image
                    {
                        Source = thumb,
                        Width = 168,
                        Height = 126,
                        Stretch = Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    var name = new TextBlock
                    {
                        Text = item.Name,
                        FontSize = 12,
                        Foreground = Application.Current.Resources["InkBrush"] as Microsoft.UI.Xaml.Media.SolidColorBrush,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        Margin = new Thickness(8, 6, 8, 7),
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    };

                    var panel = new StackPanel();
                    panel.Children.Add(image);
                    panel.Children.Add(name);
                    card.Content = panel;
                    card.Click += RecentCard_Click;

                    RecentList.Children.Add(card);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "HomePage.LoadRecent");
            }
        }

        private void RecentCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: ArtworkItem item })
            {
                Nav.Go<StudioPage>(new StudioLaunch { OpenArtwork = item });
            }
        }
    }
}
