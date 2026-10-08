using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace PaperPaint.Views
{
    /// <summary>StudioPage — save / export / share / print / open artwork.</summary>
    public sealed partial class StudioPage
    {
        // ------------------------------------------------------------------
        // launch payloads (new / coloring / photo / open)
        // ------------------------------------------------------------------

        private void ApplyLaunch(StudioLaunch launch)
        {
            if (!EngineReady)
            {
                _pendingLaunch = launch;
                return;
            }

            try
            {
                if (launch.OpenArtwork != null)
                {
                    _ = LoadArtworkAsync(launch.OpenArtwork);
                }
                else if (launch.BaseImage != null)
                {
                    ResetToBase(null, launch.BaseImage);
                }
                else if (!string.IsNullOrEmpty(launch.ColoringPageId))
                {
                    ResetToBase(launch.ColoringPageId, null);
                }
                else
                {
                    ResetToBase(null, null);
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ApplyLaunch");
            }
        }

        private void ApplyPendingLaunchOnCanvas()
        {
            if (_pendingLaunch != null)
            {
                var launch = _pendingLaunch;
                _pendingLaunch = null;
                ApplyLaunch(launch);
            }
        }

        /// <summary>Resets the painting to a blank page, a coloring page, or an imported photo.</summary>
        private void ResetToBase(string? coloringId, ImagePayload? image)
        {
            if (!EngineReady)
            {
                return;
            }

            _coloringBaseId = coloringId;
            _baseImagePixels = image?.Pixels;

            using (var ds = _baseLayer!.CreateDrawingSession())
            {
                ds.Clear(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                if (coloringId != null)
                {
                    ColoringArt.DrawPage(ds, coloringId, CanvasW, CanvasH, 8f);
                }
                else if (image != null)
                {
                    using var bmp = CanvasBitmap.CreateFromBytes(_device!, image.Pixels,
                        image.Width, image.Height, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized,
                        96, CanvasAlphaMode.Premultiplied);
                    ds.DrawImage(bmp, new Windows.Foundation.Rect(0, 0, CanvasW, CanvasH));
                }
            }

            _artworkId = null;
            _artworkName = image?.SuggestedName is string n && !string.IsNullOrWhiteSpace(n)
                ? n
                : "Untitled painting";
            _dirty = false;

            ClearPainting();
            FitPainting();
            UpdateNameText();
        }

        private async Task LoadArtworkAsync(ArtworkItem item)
        {
            try
            {
                if (!EngineReady)
                {
                    return;
                }

                var file = await ArtworkStore.GetImageFileAsync(item);
                using var stream = await file.OpenReadAsync();
                using var bmp = await CanvasBitmap.LoadAsync(_device!, stream);

                // Keep a pixel copy so the base survives a GPU device reset.
                _baseImagePixels = bmp.GetPixelBytes();
                _coloringBaseId = null;

                using (var ds = _baseLayer!.CreateDrawingSession())
                {
                    ds.Clear(Windows.UI.Color.FromArgb(0, 0, 0, 0));
                    ds.DrawImage(bmp, new Windows.Foundation.Rect(0, 0, CanvasW, CanvasH));
                }

                _artworkId = item.Id;
                _artworkName = item.Name;
                _dirty = false;

                ClearPainting();
                FitPainting();
                UpdateNameText();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "LoadArtworkAsync");
                await ShowMessageAsync("Couldn't open that painting", "The file may have been moved or deleted.");
            }
        }

        // ------------------------------------------------------------------
        // composite rendering
        // ------------------------------------------------------------------

        /// <summary>Renders paper + base + dry + wet at an arbitrary size and encodes PNG bytes.</summary>
        private async Task<byte[]> RenderCompositePngAsync(int width, int height)
        {
            using var rt = new CanvasRenderTarget(_device!, width, height, 96);
            using (var ds = rt.CreateDrawingSession())
            {
                ds.Clear(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                var dest = new Windows.Foundation.Rect(0, 0, width, height);
                var src = new Windows.Foundation.Rect(0, 0, CanvasW, CanvasH);
                ds.DrawImage(_paper!, dest, src);
                ds.DrawImage(_baseLayer!, dest, src);
                ds.DrawImage(_dry!, dest, src);
                ds.DrawImage(_wet!, dest, src);
            }

            using var ms = new InMemoryRandomAccessStream();
            await rt.SaveAsync(ms, CanvasBitmapFileFormat.Png);
            var bytes = new byte[(int)ms.Size];
            using var reader = new DataReader(ms.GetInputStreamAt(0));
            await reader.LoadAsync((uint)bytes.Length);
            reader.ReadBytes(bytes);
            return bytes;
        }

        // ------------------------------------------------------------------
        // save / export
        // ------------------------------------------------------------------

        private async Task SaveAsync()
        {
            if (_saving || !EngineReady)
            {
                return;
            }

            _saving = true;
            try
            {
                var png = await RenderCompositePngAsync((int)CanvasW, (int)CanvasH);
                var thumb = await RenderCompositePngAsync(480, 360);

                var existing = _artworkId == null ? null : new ArtworkItem { Id = _artworkId, Name = _artworkName };
                var item = await ArtworkStore.SaveArtworkAsync(png, thumb, existing);
                _artworkId = item.Id;
                _artworkName = item.Name;
                _dirty = false;
                UpdateNameText();
                ShowTransientStatus("Saved to My Artwork ✓");
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "SaveAsync");
                await ShowMessageAsync("Couldn't save", "Saving the painting failed. Please try again.");
            }
            finally
            {
                _saving = false;
            }
        }

        private async Task ExportAsync()
        {
            if (!EngineReady)
            {
                return;
            }

            try
            {
                var picker = new FileSavePicker
                {
                    SuggestedFileName = string.IsNullOrWhiteSpace(_artworkName) || _artworkName == "Untitled painting"
                        ? "My painting"
                        : _artworkName
                };
                WinRT.Interop.InitializeWithWindow.Initialize(picker, App.Hwnd);
                picker.FileTypeChoices.Add("PNG image", new System.Collections.Generic.List<string> { ".png" });
                picker.FileTypeChoices.Add("JPEG image", new System.Collections.Generic.List<string> { ".jpg" });

                var file = await picker.PickSaveFileAsync();
                if (file == null)
                {
                    return;
                }

                bool isJpeg = file.FileType.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                              file.FileType.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);

                using var rt = new CanvasRenderTarget(_device!, CanvasW, CanvasH, 96);
                using (var ds = rt.CreateDrawingSession())
                {
                    ds.Clear(Windows.UI.Color.FromArgb(255, 255, 255, 255));
                    ds.DrawImage(_paper!);
                    ds.DrawImage(_baseLayer!);
                    ds.DrawImage(_dry!);
                    ds.DrawImage(_wet!);
                }

                using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                await rt.SaveAsync(stream, isJpeg ? CanvasBitmapFileFormat.Jpeg : CanvasBitmapFileFormat.Png);
                ShowTransientStatus("Exported ✓");
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ExportAsync");
                await ShowMessageAsync("Couldn't export", "Exporting the painting failed. Please try again.");
            }
        }

        // ------------------------------------------------------------------
        // share / print
        // ------------------------------------------------------------------

        private async Task ShareSnapshotAsync()
        {
            try
            {
                if (!EngineReady)
                {
                    return;
                }

                var png = await RenderCompositePngAsync((int)CanvasW, (int)CanvasH);
                string tempDir = Path.Combine(Path.GetTempPath(), "PaperPaint");
                Directory.CreateDirectory(tempDir);
                var temp = await StorageFolder.GetFolderFromPathAsync(tempDir);
                var file = await temp.CreateFileAsync("paperpaint_share.png", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, png);

                bool shared = await SharePrintService.ShareAsync(file, _artworkName);
                if (!shared)
                {
                    await ShowMessageAsync(
                        "Sharing isn't available",
                        "The system share sheet could not be opened here. Use Export to save a copy, then share it from your photos.");
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "ShareSnapshotAsync");
            }
        }

        private async Task PrintAsync()
        {
            try
            {
                if (!EngineReady)
                {
                    return;
                }

                var png = await RenderCompositePngAsync((int)CanvasW, (int)CanvasH);
                string tempDir = Path.Combine(Path.GetTempPath(), "PaperPaint");
                Directory.CreateDirectory(tempDir);
                var temp = await StorageFolder.GetFolderFromPathAsync(tempDir);
                var file = await temp.CreateFileAsync("paperpaint_print.png", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(file, png);

                bool printed = await SharePrintService.PrintAsync(XamlRoot, file, _artworkName);
                if (!printed)
                {
                    await ShowMessageAsync(
                        "Printing isn't available",
                        "The print dialog could not be opened. Use Export to save a copy and print it from another app.");
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "PrintAsync");
            }
        }

        // ------------------------------------------------------------------
        // dialogs
        // ------------------------------------------------------------------

        private async Task ShowMessageAsync(string title, string message)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                _dialogOpen = true;
                await dialog.ShowAsync();
            }
            finally
            {
                _dialogOpen = false;
            }
        }

        private async Task<bool> ConfirmAsync(string title, string message, string primaryText)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = primaryText,
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };
                _dialogOpen = true;
                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
            finally
            {
                _dialogOpen = false;
            }
        }
    }
}
