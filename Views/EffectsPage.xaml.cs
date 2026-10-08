using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI;

namespace PaperPaint.Views
{
    /// <summary>Import &amp; Effects: preview an imported/captured photo, try effects, then send it to the Paint Studio.</summary>
    public sealed partial class EffectsPage : Page
    {
        private ImagePayload? _source;
        private readonly Dictionary<PhotoEffect, ImageSource> _previewCache = new Dictionary<PhotoEffect, ImageSource>();
        private readonly Dictionary<PhotoEffect, Button> _tiles = new Dictionary<PhotoEffect, Button>();
        private PhotoEffect _selected = PhotoEffect.None;
        private bool _starting;

        private const int PreviewW = 800;
        private const int PreviewH = 600;
        private const int ThumbW = 200;
        private const int ThumbH = 150;

        public EffectsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Nav.PendingArg is ImagePayload payload)
            {
                Nav.PendingArg = null;
                _source = payload;
                SourceNameText.Text = string.IsNullOrWhiteSpace(payload.SuggestedName)
                    ? "Imported photo"
                    : payload.SuggestedName;

                try
                {
                    await BuildPreviewsAsync();
                }
                catch (Exception ex)
                {
                    AppLogger.LogException(ex, "EffectsPage.BuildPreviews");
                }
            }
            else if (_source == null)
            {
                // No photo in flight — nothing to do here.
                Nav.Go<HomePage>();
            }
        }

        private async Task BuildPreviewsAsync()
        {
            if (_source == null)
            {
                return;
            }

            // Cheap working copies for instant preview + tiles.
            byte[] preview = ImageOps.ResizeBilinear(_source.Pixels, _source.Width, _source.Height, PreviewW, PreviewH);
            byte[] thumbSrc = ImageOps.ResizeBilinear(_source.Pixels, _source.Width, _source.Height, ThumbW, ThumbH);

            EffectList.Children.Clear();
            _tiles.Clear();
            _previewCache.Clear();

            var effects = new[] { PhotoEffect.None, PhotoEffect.Sketch, PhotoEffect.Blur, PhotoEffect.Sepia, PhotoEffect.BlackWhite };
            foreach (var effect in effects)
            {
                var previewBytes = await ImageEffects.ApplyAsync(effect, preview, PreviewW, PreviewH);
                var previewImage = await PngToImageAsync(previewBytes, PreviewW, PreviewH);
                _previewCache[effect] = previewImage;

                var thumbBytes = await ImageEffects.ApplyAsync(effect, thumbSrc, ThumbW, ThumbH);
                var thumbImage = await PngToImageAsync(thumbBytes, ThumbW, ThumbH);

                var tile = BuildTile(effect, thumbImage);
                EffectList.Children.Add(tile);
            }

            Select(PhotoEffect.None);
        }

        private static async Task<ImageSource> PngToImageAsync(byte[] bgra, int w, int h)
        {
            var png = await ImageOps.EncodePngAsync(bgra, w, h);
            using var ms = new InMemoryRandomAccessStream();
            await ms.WriteAsync(png.AsBuffer());
            ms.Seek(0);

            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(ms);
            return bmp;
        }

        private Button BuildTile(PhotoEffect effect, ImageSource thumb)
        {
            var label = new TextBlock
            {
                Text = ImageEffects.DisplayName(effect),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = Application.Current.Resources["InkBrush"] as SolidColorBrush,
                VerticalAlignment = VerticalAlignment.Center
            };

            var image = new Image
            {
                Source = thumb,
                Width = 84,
                Height = 63,
                Stretch = Stretch.UniformToFill
            };

            var imageFrame = new Border
            {
                Child = image,
                CornerRadius = new CornerRadius(4)
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
            row.Children.Add(imageFrame);
            row.Children.Add(label);

            var tile = new Button
            {
                Tag = effect,
                Content = row,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 8, 10, 8),
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                BorderBrush = Application.Current.Resources["LineBrush"] as SolidColorBrush,
                BorderThickness = new Thickness(1)
            };
            AutomationProperties.SetName(tile, $"Effect {ImageEffects.DisplayName(effect)}");
            tile.Click += Tile_Click;
            _tiles[effect] = tile;
            return tile;
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { Tag: PhotoEffect effect })
            {
                Select(effect);
            }
        }

        private void Select(PhotoEffect effect)
        {
            _selected = effect;

            foreach (var (key, tile) in _tiles)
            {
                bool on = key == effect;
                tile.BorderBrush = on
                    ? Application.Current.Resources["AccentBrush"] as SolidColorBrush
                    : Application.Current.Resources["LineBrush"] as SolidColorBrush;
                tile.BorderThickness = on ? new Thickness(2) : new Thickness(1);
                tile.Background = on
                    ? Application.Current.Resources["AccentSoftBrush"] as SolidColorBrush
                    : new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }

            if (_previewCache.TryGetValue(effect, out var source))
            {
                PreviewImage.Source = source;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Nav.Go<HomePage>();

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_starting || _source == null)
            {
                return;
            }

            _starting = true;
            StartButton.IsEnabled = false;
            BusyRow.Visibility = Visibility.Visible;

            try
            {
                // Apply the chosen effect at full painting resolution off the UI thread.
                var processed = _selected == PhotoEffect.None
                    ? _source.Pixels
                    : await ImageEffects.ApplyAsync(_selected, _source.Pixels, _source.Width, _source.Height);

                var payload = new ImagePayload
                {
                    Pixels = processed,
                    Width = _source.Width,
                    Height = _source.Height,
                    SuggestedName = _source.SuggestedName
                };

                Nav.Go<StudioPage>(new StudioLaunch { BaseImage = payload });
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "EffectsPage.Start");
            }
            finally
            {
                _starting = false;
                StartButton.IsEnabled = true;
                BusyRow.Visibility = Visibility.Collapsed;
            }
        }
    }
}
