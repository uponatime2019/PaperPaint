using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;

namespace PaperPaint.Views
{
    /// <summary>Card shown in the My Artwork grid.</summary>
    public sealed class ArtworkCard
    {
        public ArtworkItem Item { get; }
        public ImageSource? Thumb { get; }
        public string Subtitle { get; }

        public ArtworkCard(ArtworkItem item, ImageSource? thumb)
        {
            Item = item;
            Thumb = thumb;
            Subtitle = FormatWhen(item.ModifiedUtc != default ? item.ModifiedUtc : item.CreatedUtc);
        }

        private static string FormatWhen(DateTime utc)
        {
            var local = utc.ToLocalTime();
            var today = DateTime.Now.Date;
            if (local.Date == today)
            {
                return $"Today {local:t}";
            }

            if (local.Date == today.AddDays(-1))
            {
                return $"Yesterday {local:t}";
            }

            return local.ToString("d");
        }
    }

    /// <summary>My Artwork: saved paintings with open / rename / delete.</summary>
    public sealed partial class ArtworkPage : Page
    {
        private readonly List<ArtworkCard> _cards = new List<ArtworkCard>();

        public ArtworkPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var items = (await ArtworkStore.LoadIndexAsync())
                    .OrderByDescending(x => x.ModifiedUtc)
                    .ToList();

                _cards.Clear();
                foreach (var item in items)
                {
                    BitmapImage? thumb = null;
                    var stream = await ArtworkStore.OpenThumbnailAsync(item);
                    if (stream != null)
                    {
                        thumb = new BitmapImage();
                        await thumb.SetSourceAsync(stream);
                    }

                    _cards.Add(new ArtworkCard(item, thumb));
                }

                ArtworkGrid.ItemsSource = null;
                ArtworkGrid.ItemsSource = _cards;
                CountText.Text = _cards.Count == 0
                    ? "Saved paintings live on this device."
                    : $"{_cards.Count} painting{(_cards.Count == 1 ? "" : "s")}";
                EmptyState.Visibility = _cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ArtworkPage.Load");
            }
        }

        private void ArtworkGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ArtworkCard card)
            {
                Nav.Go<StudioPage>(new StudioLaunch { OpenArtwork = card.Item });
            }
        }

        private void NewPainting_Click(object sender, RoutedEventArgs e)
        {
            Nav.Go<StudioPage>(new StudioLaunch());
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Flyout opens automatically; handler kept for future actions.
        }

        private async void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ArtworkCard card })
            {
                return;
            }

            var input = new TextBox
            {
                Header = "Painting name",
                Text = card.Item.Name,
                PlaceholderText = "My painting"
            };

            var dialog = new ContentDialog
            {
                Title = "Rename painting",
                Content = input,
                PrimaryButtonText = "Rename",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            var name = input.Text?.Trim();
            if (string.IsNullOrEmpty(name) || name == card.Item.Name)
            {
                return;
            }

            try
            {
                await ArtworkStore.RenameAsync(card.Item, name);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ArtworkPage.Rename");
            }
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: ArtworkCard card })
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Delete painting?",
                Content = $"“{card.Item.Name}” will be removed from My Artwork. This can't be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Keep",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            try
            {
                await ArtworkStore.DeleteAsync(card.Item);
                await LoadAsync();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ArtworkPage.Delete");
            }
        }
    }
}
