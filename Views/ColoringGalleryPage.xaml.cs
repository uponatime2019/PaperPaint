using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.UI;

namespace PaperPaint.Views
{
    /// <summary>Card shown in the coloring gallery grid.</summary>
    public sealed class ColoringCard
    {
        public ColoringPageDef Def { get; }
        public ImageSource Thumb { get; }

        public ColoringCard(ColoringPageDef def, ImageSource thumb)
        {
            Def = def;
            Thumb = thumb;
        }
    }

    /// <summary>Built-in original coloring pages with category filter; tapping a page opens it in the Paint Studio.</summary>
    public sealed partial class ColoringGalleryPage : Page
    {
        private readonly Dictionary<string, List<ColoringCard>> _cardsByCategory = new Dictionary<string, List<ColoringCard>>();
        private string _selectedCategory = "All";
        private Button? _selectedChip;
        private bool _thumbsBuilt;

        public ColoringGalleryPage()
        {
            InitializeComponent();
            BuildFilterChips();
            Loaded += Page_Loaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_thumbsBuilt)
            {
                return;
            }

            _thumbsBuilt = true;
            try
            {
                await BuildThumbnailsAsync();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ColoringGallery.BuildThumbnails");
            }
        }

        private void BuildFilterChips()
        {
            FilterRow.Children.Clear();
            var categories = new[] { "All" }.Concat(ColoringArt.Categories).ToArray();
            foreach (var category in categories)
            {
                var chip = new Button
                {
                    Tag = category,
                    Content = category,
                    Height = 32,
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(16, 0, 16, 0),
                    FontSize = 13,
                    Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    BorderBrush = Application.Current.Resources["LineBrush"] as SolidColorBrush,
                    BorderThickness = new Thickness(1),
                    Foreground = Application.Current.Resources["InkBrush"] as SolidColorBrush
                };
                AutomationProperties.SetName(chip, $"Filter by {category}");
                chip.Click += Chip_Click;
                FilterRow.Children.Add(chip);

                if (category == _selectedCategory)
                {
                    StyleChip(chip, true);
                    _selectedChip = chip;
                }
            }
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string category })
            {
                return;
            }

            if (_selectedChip != null)
            {
                StyleChip(_selectedChip, false);
            }

            _selectedChip = (Button)sender;
            StyleChip(_selectedChip, true);
            _selectedCategory = category;
            ApplyFilter();
        }

        private void StyleChip(Button chip, bool selected)
        {
            if (selected)
            {
                chip.Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
                chip.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                chip.BorderBrush = Application.Current.Resources["AccentBrush"] as SolidColorBrush;
            }
            else
            {
                chip.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                chip.Foreground = Application.Current.Resources["InkBrush"] as SolidColorBrush;
                chip.BorderBrush = Application.Current.Resources["LineBrush"] as SolidColorBrush;
            }
        }

        private async Task BuildThumbnailsAsync()
        {
            var device = CanvasDevice.GetSharedDevice();

            foreach (var def in ColoringArt.Pages)
            {
                var thumb = await RenderThumbnailAsync(device, def.Id);
                var card = new ColoringCard(def, thumb);

                if (!_cardsByCategory.TryGetValue(def.Category, out var list))
                {
                    list = new List<ColoringCard>();
                    _cardsByCategory[def.Category] = list;
                }

                list.Add(card);
            }

            ApplyFilter();
        }

        private static async Task<ImageSource> RenderThumbnailAsync(CanvasDevice device, string id)
        {
            using var rt = new CanvasRenderTarget(device, 392, 294, 96);
            using (var ds = rt.CreateDrawingSession())
            {
                ds.Clear(Color.FromArgb(255, 255, 255, 255));
                ColoringArt.DrawPage(ds, id, 392f, 294f, 12f);
            }

            using var ms = new InMemoryRandomAccessStream();
            await rt.SaveAsync(ms, Microsoft.Graphics.Canvas.CanvasBitmapFileFormat.Png);

            var bmp = new BitmapImage();
            await bmp.SetSourceAsync(ms);
            return bmp;
        }

        private void ApplyFilter()
        {
            var cards = _selectedCategory == "All"
                ? _cardsByCategory.Values.SelectMany(v => v).ToList()
                : _cardsByCategory.TryGetValue(_selectedCategory, out var list) ? list : new List<ColoringCard>();

            PagesGrid.ItemsSource = cards;
        }

        private void PagesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ColoringCard card)
            {
                Nav.Go<StudioPage>(new StudioLaunch { ColoringPageId = card.Def.Id });
            }
        }
    }
}
