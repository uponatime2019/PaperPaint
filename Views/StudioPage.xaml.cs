using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaperPaint.Helpers;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.System;
using Windows.UI;

namespace PaperPaint.Views
{
    /// <summary>Paint Studio: brush panel, palette, commands and shortcuts. Engine lives in StudioPage.Paint.cs, file IO in StudioPage.IO.cs.</summary>
    public sealed partial class StudioPage : Page
    {
        // painting settings
        private PaintTool _tool = PaintTool.Pen;
        private Color _currentColor = Color.FromArgb(255, 32, 27, 22);
        private float _brushSize = 14f;
        private float _brushOpacity = 1f;

        // session state
        private bool _dirty;
        private bool _neverFitted = true;
        private bool _saving;
        private bool _dialogOpen;
        private byte[]? _baseImagePixels;
        private string? _artworkId;
        private string _artworkName = "Untitled painting";
        private StudioLaunch? _pendingLaunch;

        // palette
        private readonly List<Color> _palette = new List<Color>();
        private Button? _selectedSwatch;
        private Microsoft.UI.Dispatching.DispatcherQueueTimer? _statusTimer;

        private static readonly Color[] DefaultPalette = new Color[]
        {
            Color.FromArgb(255, 32, 27, 22),   Color.FromArgb(255, 74, 66, 58),    Color.FromArgb(255, 138, 128, 120), Color.FromArgb(255, 200, 192, 180), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 157, 116, 85),
            Color.FromArgb(255, 217, 59, 43),  Color.FromArgb(255, 232, 116, 59),  Color.FromArgb(255, 245, 166, 35),   Color.FromArgb(255, 247, 209, 84),  Color.FromArgb(255, 251, 235, 200), Color.FromArgb(255, 226, 213, 183),
            Color.FromArgb(255, 63, 166, 106), Color.FromArgb(255, 44, 122, 75),   Color.FromArgb(255, 163, 162, 75),   Color.FromArgb(255, 185, 228, 206), Color.FromArgb(255, 46, 155, 181),   Color.FromArgb(255, 111, 183, 232),
            Color.FromArgb(255, 59, 111, 232), Color.FromArgb(255, 43, 75, 155),   Color.FromArgb(255, 93, 116, 128),   Color.FromArgb(255, 124, 92, 196),  Color.FromArgb(255, 142, 69, 133),  Color.FromArgb(255, 199, 79, 160),
            Color.FromArgb(255, 242, 168, 196),Color.FromArgb(255, 240, 138, 120), Color.FromArgb(255, 244, 182, 160),  Color.FromArgb(255, 210, 169, 122), Color.FromArgb(255, 203, 184, 232), Color.FromArgb(255, 94, 176, 160),
        };

        public StudioPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            LoadPalette();
            BuildPaletteUI();
            WireAccelerators();

            ToolPen.IsChecked = true;
            UpdateZoomText();
            UpdateCommandStates();
            UpdateNameText();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Nav.PendingArg != null)
            {
                var arg = Nav.PendingArg;
                Nav.PendingArg = null;
                if (arg is StudioLaunch launch)
                {
                    ApplyLaunch(launch);
                }
            }

            if (_neverFitted && EngineReady)
            {
                _neverFitted = false;
                FitPainting();
            }
        }

        // ------------------------------------------------------------------
        // palette
        // ------------------------------------------------------------------

        private void LoadPalette()
        {
            _palette.AddRange(DefaultPalette);
            try
            {
                if (AppSettingsHelper.GetString("CustomPaletteColors") is string custom &&
                    !string.IsNullOrWhiteSpace(custom))
                {
                    foreach (var part in custom.Split(','))
                    {
                        if (TryParseColor(part.Trim(), out var c) && !_palette.Contains(c))
                        {
                            _palette.Add(c);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // custom palette is best-effort
            }
        }

        private static bool TryParseColor(string text, out Color color)
        {
            try
            {
                if (text.StartsWith("#", StringComparison.Ordinal))
                {
                    text = text[1..];
                }

                if (uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out uint value))
                {
                    if (text.Length == 6)
                    {
                        value |= 0xFF000000;
                    }

                    color = Color.FromArgb((byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF));
                    return true;
                }
            }
            catch (Exception)
            {
            }

            color = default;
            return false;
        }

        private void BuildPaletteUI()
        {
            PaletteHost.Children.Clear();
            PaletteHost.RowDefinitions.Clear();
            PaletteHost.ColumnDefinitions.Clear();

            const int columns = 6;
            int rows = (int)Math.Ceiling(_palette.Count / (double)columns);
            for (int r = 0; r < rows; r++)
            {
                PaletteHost.RowDefinitions.Add(new RowDefinition());
            }

            for (int c = 0; c < columns; c++)
            {
                PaletteHost.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < _palette.Count; i++)
            {
                var color = _palette[i];
                var swatch = new Button
                {
                    Tag = color,
                    Width = 30,
                    Height = 30,
                    MinWidth = 0,
                    MinHeight = 0,
                    Padding = new Thickness(3),
                    Margin = new Thickness(1),
                    CornerRadius = new CornerRadius(15),
                    Background = new SolidColorBrush(Color.FromArgb(255, 250, 247, 240)),
                    BorderThickness = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                AutomationProperties.SetName(swatch, $"Color {FormatColor(color)}");
                var dot = new Border
                {
                    Width = 20,
                    Height = 20,
                    CornerRadius = new CornerRadius(10),
                    Background = new SolidColorBrush(color),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                swatch.Content = dot;
                swatch.Click += Swatch_Click;
                ToolTipService.SetToolTip(swatch, FormatColor(color));

                Grid.SetRow(swatch, i / columns);
                Grid.SetColumn(swatch, i % columns);
                PaletteHost.Children.Add(swatch);

                if (color == _currentColor)
                {
                    SelectSwatch(swatch);
                }
            }
        }

        private static string FormatColor(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private void Swatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is Color c)
            {
                _currentColor = c;
                SelectSwatch(b);
            }
        }

        private void SelectSwatch(Button swatch)
        {
            if (_selectedSwatch != null)
            {
                _selectedSwatch.BorderThickness = new Thickness(0);
            }

            _selectedSwatch = swatch;
            _selectedSwatch.BorderThickness = new Thickness(2);
            _selectedSwatch.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 232, 116, 59));
        }

        private void AddColorButton_Click(object sender, RoutedEventArgs e)
        {
            CustomColorPicker.Color = _currentColor;
            ColorFlyout.ShowAt(AddColorButton);
        }

        private void CustomColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            // live preview only; committed on "Add to palette"
        }

        private void ConfirmAddColor_Click(object sender, RoutedEventArgs e)
        {
            var c = CustomColorPicker.Color;
            if (!_palette.Contains(c))
            {
                _palette.Add(c);
                SaveCustomPalette();
                BuildPaletteUI();

                // select the new swatch (last one)
                var last = PaletteHost.Children.OfType<Button>().LastOrDefault(b => b.Tag is Color cc && cc == c);
                if (last != null)
                {
                    _currentColor = c;
                    SelectSwatch(last);
                }
            }
            else
            {
                _currentColor = c;
                var match = PaletteHost.Children.OfType<Button>().FirstOrDefault(b => b.Tag is Color cc && cc == c);
                if (match != null)
                {
                    SelectSwatch(match);
                }
            }

            ColorFlyout.Hide();
        }

        private void SaveCustomPalette()
        {
            try
            {
                var custom = _palette.Skip(DefaultPalette.Length)
                    .Select(FormatColorWithAlpha)
                    .ToArray();
                AppSettingsHelper.SetString("CustomPaletteColors", string.Join(",", custom));
            }
            catch (Exception)
            {
            }
        }

        private static string FormatColorWithAlpha(Color c) => $"{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

        // ------------------------------------------------------------------
        // tool / sliders
        // ------------------------------------------------------------------

        private void ToolButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton tb || tb.Tag is not string tag)
            {
                return;
            }

            foreach (var other in new[] { ToolPen, ToolPencil, ToolWatercolor, ToolOil, ToolPastel, ToolEraser })
            {
                if (!ReferenceEquals(other, tb) && other != null && other.IsChecked == true)
                {
                    other.IsChecked = false;
                }
            }

            _tool = Enum.TryParse<PaintTool>(tag, out var parsed) ? parsed : PaintTool.Pen;
        }

        private void SelectTool(PaintTool tool)
        {
            var target = tool switch
            {
                PaintTool.Pen => ToolPen,
                PaintTool.Pencil => ToolPencil,
                PaintTool.Watercolor => ToolWatercolor,
                PaintTool.Oil => ToolOil,
                PaintTool.Pastel => ToolPastel,
                PaintTool.Eraser => ToolEraser,
                _ => ToolPen
            };
            target.IsChecked = true;
        }

        private void SizeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _brushSize = (float)e.NewValue;
            if (SizeValueText != null)
            {
                SizeValueText.Text = $"{(int)e.NewValue} px";
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _brushOpacity = (float)(e.NewValue / 100.0);
            if (OpacityValueText != null)
            {
                OpacityValueText.Text = $"{(int)e.NewValue}%";
            }
        }

        // ------------------------------------------------------------------
        // command buttons
        // ------------------------------------------------------------------

        private void HomeButton_Click(object sender, RoutedEventArgs e) => Nav.Go<HomePage>();

        private void ColoringButton_Click(object sender, RoutedEventArgs e) => Nav.Go<ColoringGalleryPage>();

        private void RailUndo_Click(object sender, RoutedEventArgs e) => Undo();

        private void RailRedo_Click(object sender, RoutedEventArgs e) => Redo();

        private void DryButton_Click(object sender, RoutedEventArgs e) => DryPaint();

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (_strokes.Count == 0 && _baseImagePixels == null && _coloringBaseId == null)
            {
                return;
            }

            bool clear = await ConfirmAsync(
                "Clear the canvas?",
                "This wipes the current painting. A coloring page keeps its line art.",
                "Clear");
            if (!clear)
            {
                return;
            }

            if (_coloringBaseId is string pageId)
            {
                ClearPainting();
                try
                {
                    using var ds = _baseLayer!.CreateDrawingSession();
                    ds.Clear(Color.FromArgb(0, 0, 0, 0));
                    ColoringArt.DrawPage(ds, pageId, CanvasW, CanvasH, 8f);
                    PaintCanvas.Invalidate();
                }
                catch (Exception ex)
                {
                    AppLogger.LogException(ex, "ClearButton redraw base");
                }
            }
            else if (_baseImagePixels != null)
            {
                ResetToBase(null, null);
            }
            else
            {
                ClearPainting();
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e) => await SaveAsync();

        private async void ExportButton_Click(object sender, RoutedEventArgs e) => await ExportAsync();

        private async void PrintButton_Click(object sender, RoutedEventArgs e) => await PrintAsync();

        private async void ShareButton_Click(object sender, RoutedEventArgs e) => await ShareSnapshotAsync();

        private void FitButton_Click(object sender, RoutedEventArgs e) => FitPainting();

        private void ZoomInButton_Click(object sender, RoutedEventArgs e) => ZoomStep(1.25f);

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e) => ZoomStep(1f / 1.25f);

        private void ZoomResetButton_Click(object sender, RoutedEventArgs e) => ResetZoom();

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var payload = await PickPhotoAsync();
            if (payload != null)
            {
                Nav.Go<EffectsPage>(payload);
            }
        }

        private async void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CameraDialog { XamlRoot = XamlRoot };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary && dialog.Result != null)
            {
                Nav.Go<EffectsPage>(dialog.Result);
            }
        }

        private static async Task<ImagePayload?> PickPhotoAsync()
        {
            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                WinRT.Interop.InitializeWithWindow.Initialize(picker, App.Hwnd);
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".bmp");

                var file = await picker.PickSingleFileAsync();
                if (file == null)
                {
                    return null;
                }

                using var stream = await file.OpenReadAsync();
                return await ImageOps.DecodeFittedAsync(stream, System.IO.Path.GetFileNameWithoutExtension(file.Name));
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "PickPhotoAsync");
                return null;
            }
        }

        // ------------------------------------------------------------------
        // status / state UI
        // ------------------------------------------------------------------

        private void UpdateCommandStates()
        {
            var canUndo = _strokes.Count > 0 && _activeStroke == null;
            var canRedo = _redoStack.Count > 0 && _activeStroke == null;

            if (RailUndoButton != null)
            {
                RailUndoButton.IsEnabled = canUndo;
                BottomUndo.IsEnabled = canUndo;
                RailRedoButton.IsEnabled = canRedo;
                BottomRedo.IsEnabled = canRedo;
                BottomDry.IsEnabled = HasWetPaint;
                DryButton.IsEnabled = HasWetPaint;
                WetChip.Visibility = HasWetPaint ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateZoomText()
        {
            if (ZoomText != null)
            {
                ZoomText.Text = $"{(int)Math.Round(_scale * 100)}%";
            }
        }

        private void UpdateNameText()
        {
            if (PaintingNameText != null)
            {
                PaintingNameText.Text = _dirty ? $"{_artworkName} •" : _artworkName;
            }
        }

        private void ShowTransientStatus(string message)
        {
            if (PaintingNameText == null)
            {
                return;
            }

            PaintingNameText.Text = message;
            _statusTimer?.Stop();
            _statusTimer = DispatcherQueue.CreateTimer();
            _statusTimer.Interval = TimeSpan.FromSeconds(2.2);
            _statusTimer.Tick += (_, _) =>
            {
                _statusTimer.Stop();
                UpdateNameText();
            };
            _statusTimer.Start();
        }

        // ------------------------------------------------------------------
        // keyboard shortcuts
        // ------------------------------------------------------------------

        private void WireAccelerators()
        {
            void Add(VirtualKey key, VirtualKeyModifiers mods)
            {
                var accel = new KeyboardAccelerator
                {
                    Key = key,
                    Modifiers = mods
                };
                accel.Invoked += Accel_Invoked;
                KeyboardAccelerators.Add(accel);
            }

            Add(VirtualKey.Z, VirtualKeyModifiers.Control);
            Add(VirtualKey.Y, VirtualKeyModifiers.Control);
            Add(VirtualKey.Z, VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift);
            Add(VirtualKey.S, VirtualKeyModifiers.Control);
            Add(VirtualKey.D, VirtualKeyModifiers.Control);
            Add(VirtualKey.Number0, VirtualKeyModifiers.Control);
            Add(VirtualKey.E, VirtualKeyModifiers.Control);
            Add(VirtualKey.O, VirtualKeyModifiers.Control);
            Add(VirtualKey.P, VirtualKeyModifiers.Control);
            Add(VirtualKey.Number1, VirtualKeyModifiers.None);
            Add(VirtualKey.Number2, VirtualKeyModifiers.None);
            Add(VirtualKey.Number3, VirtualKeyModifiers.None);
            Add(VirtualKey.Number4, VirtualKeyModifiers.None);
            Add(VirtualKey.Number5, VirtualKeyModifiers.None);
            Add(VirtualKey.Number6, VirtualKeyModifiers.None);
        }

        private void Accel_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (_dialogOpen)
            {
                return;
            }

            var key = sender.Key;
            var mods = sender.Modifiers;
            bool ctrl = mods.HasFlag(VirtualKeyModifiers.Control);

            try
            {
                if (ctrl && mods.HasFlag(VirtualKeyModifiers.Shift) && key == VirtualKey.Z)
                {
                    Redo();
                }
                else if (ctrl && key == VirtualKey.Z)
                {
                    Undo();
                }
                else if (ctrl && key == VirtualKey.Y)
                {
                    Redo();
                }
                else if (ctrl && key == VirtualKey.S)
                {
                    _ = SaveAsync();
                }
                else if (ctrl && key == VirtualKey.D)
                {
                    DryPaint();
                }
                else if (ctrl && key == VirtualKey.Number0)
                {
                    FitPainting();
                }
                else if (ctrl && key == VirtualKey.E)
                {
                    _ = ExportAsync();
                }
                else if (ctrl && key == VirtualKey.O)
                {
                    _ = PickPhotoAsync().ContinueWith(t =>
                    {
                        if (t.Result != null)
                        {
                            DispatcherQueue.TryEnqueue(() => Nav.Go<EffectsPage>(t.Result));
                        }
                    });
                }
                else if (ctrl && key == VirtualKey.P)
                {
                    _ = PrintAsync();
                }
                else if (!ctrl)
                {
                    SelectTool(key switch
                    {
                        VirtualKey.Number1 => PaintTool.Pen,
                        VirtualKey.Number2 => PaintTool.Pencil,
                        VirtualKey.Number3 => PaintTool.Watercolor,
                        VirtualKey.Number4 => PaintTool.Oil,
                        VirtualKey.Number5 => PaintTool.Pastel,
                        VirtualKey.Number6 => PaintTool.Eraser,
                        _ => _tool
                    });
                }
                else
                {
                    return;
                }

                args.Handled = true;
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "Accel_Invoked");
            }
        }
    }
}
