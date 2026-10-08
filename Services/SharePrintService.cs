using System;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace PaperPaint.Services
{
    /// <summary>System share sheet and Windows printing for a finished painting.</summary>
    public static class SharePrintService
    {
        // ---------------- Share ----------------

        private static bool _shareWired;
        private static StorageFile? _pendingShareFile;
        private static string _pendingShareTitle = "My painting";

        /// <summary>Shows the system share UI for an image file. Returns false when sharing is unavailable.</summary>
        public static Task<bool> ShareAsync(StorageFile imageFile, string title)
        {
            try
            {
                _pendingShareFile = imageFile;
                _pendingShareTitle = string.IsNullOrWhiteSpace(title) ? "My painting" : title;

                var manager = DataTransferManager.GetForCurrentView();
                if (!_shareWired)
                {
                    manager.DataRequested += OnDataRequested;
                    _shareWired = true;
                }

                DataTransferManager.ShowShareUI();
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ShareAsync");
                return Task.FromResult(false);
            }
        }

        private static void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            try
            {
                var request = args.Request;
                request.Data.Properties.Title = _pendingShareTitle;
                request.Data.Properties.Description = "Shared from Paper Paint";
                if (_pendingShareFile != null)
                {
                    request.Data.SetBitmap(RandomAccessStreamReference.CreateFromFile(_pendingShareFile));
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "OnDataRequested");
            }
        }

        // ---------------- Print ----------------

        /// <summary>Prints a single-page image through the Windows print dialog. Returns false when printing fails.</summary>
        public static async Task<bool> PrintAsync(XamlRoot xamlRoot, StorageFile imageFile, string jobName)
        {
            try
            {
                var bitmap = new BitmapImage();
                using (var stream = await imageFile.OpenReadAsync())
                {
                    await bitmap.SetSourceAsync(stream);
                }

                var printDocument = new Microsoft.UI.Xaml.Printing.PrintDocument();
                var source = printDocument.DocumentSource;
                var pageSize = new Size(1, 1);

                printDocument.Paginate += (s, e) =>
                {
                    pageSize = e.PrintTaskOptions.GetPageDescription(0).PageSize;
                    printDocument.SetPreviewPageCount(1, Microsoft.UI.Xaml.Printing.PreviewPageCountType.Final);
                };

                printDocument.GetPreviewPage += (s, e) =>
                {
                    var page = BuildPrintPage(bitmap, pageSize);
                    printDocument.SetPreviewPage(0, page);
                };

                printDocument.AddPages += (s, e) =>
                {
                    var page = BuildPrintPage(bitmap, pageSize);
                    printDocument.AddPage(page);
                    printDocument.AddPagesComplete();
                };

                var manager = Windows.Graphics.Printing.PrintManager.GetForCurrentView();
                bool taskHandled = false;
                TypedEventHandler<Windows.Graphics.Printing.PrintManager, Windows.Graphics.Printing.PrintTaskRequestedEventArgs> handler =
                    (s, e) =>
                    {
                        taskHandled = true;
                        var task = e.Request.CreatePrintTask(
                            string.IsNullOrWhiteSpace(jobName) ? "Paper Paint painting" : jobName,
                            args2 => args2.SetSource(source));
                    };

                manager.PrintTaskRequested += handler;
                try
                {
                    await Windows.Graphics.Printing.PrintManager.ShowPrintUIAsync();
                }
                finally
                {
                    // Give the print dialog a moment to raise the task request before unwiring.
                    await Task.Delay(1500);
                    manager.PrintTaskRequested -= handler;
                }

                return taskHandled;
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "PrintAsync");
                return false;
            }
        }

        private static Grid BuildPrintPage(BitmapImage bitmap, Size pageSize)
        {
            const double margin = 24;
            double availW = Math.Max(1, pageSize.Width - margin * 2);
            double availH = Math.Max(1, pageSize.Height - margin * 2);
            double scale = Math.Min(availW / Math.Max(1, bitmap.PixelWidth), availH / Math.Max(1, bitmap.PixelHeight));

            var image = new Image
            {
                Source = bitmap,
                Stretch = Stretch.Uniform,
                Width = Math.Max(1, bitmap.PixelWidth * scale),
                Height = Math.Max(1, bitmap.PixelHeight * scale)
            };

            var grid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Width = pageSize.Width,
                Height = pageSize.Height
            };
            var host = new Border
            {
                Child = image,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(margin)
            };
            grid.Children.Add(host);
            return grid;
        }
    }
}
