using System;
using System.Linq;
using PaperPaint.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Core;

namespace PaperPaint.Views
{
    /// <summary>Camera capture dialog: live preview via MediaFrameSource, graceful fallback when no camera / no permission.</summary>
    public sealed partial class CameraDialog : ContentDialog
    {
        private MediaCapture? _capture;
        private MediaFrameReader? _reader;
        private MediaSource? _mediaSource;
        private SoftwareBitmap? _latestFrame;
        private bool _starting;
        private bool _failed;

        /// <summary>Set when the dialog closes with a captured photo (fitted to the painting canvas).</summary>
        public Models.ImagePayload? Result { get; private set; }

        public CameraDialog()
        {
            InitializeComponent();
        }

        private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (_starting || _capture != null)
            {
                return;
            }

            _starting = true;
            try
            {
                await StartCameraAsync();
            }
            catch (UnauthorizedAccessException)
            {
                Fail("Camera access is turned off. Allow it in Windows Settings › Privacy & security › Camera, then reopen the camera.");
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "CameraDialog.StartCamera");
                Fail("The camera couldn't be started. " + ex.Message);
            }
            finally
            {
                _starting = false;
            }
        }

        private async System.Threading.Tasks.Task StartCameraAsync()
        {
            var groups = await MediaFrameSourceGroup.FindAllAsync();
            var group = groups.FirstOrDefault(g =>
                g.SourceInfos.Any(s => s.SourceKind == MediaFrameSourceKind.Color));

            if (group == null)
            {
                Fail("No camera was found on this device.");
                return;
            }

            _capture = new MediaCapture();
            await _capture.InitializeAsync(new MediaCaptureInitializationSettings
            {
                SourceGroup = group,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            });

            var source = _capture.FrameSources.Values
                .FirstOrDefault(s => s.Info?.SourceKind == MediaFrameSourceKind.Color);
            if (source == null)
            {
                Fail("No usable camera stream was found.");
                return;
            }

            _reader = await _capture.CreateFrameReaderAsync(source);
            _reader.FrameArrived += Reader_FrameArrived;
            var status = await _reader.StartAsync();
            if (status != MediaFrameReaderStartStatus.Success)
            {
                Fail("The camera stream refused to start (" + status + ").");
                return;
            }

            _mediaSource = MediaSource.CreateFromMediaFrameSource(source);
            Preview.Source = _mediaSource;

            Busy.IsActive = false;
            Hint.Text = "Frame is grabbed live — choose “Use photo” to paint on it.";
        }

        private void Reader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            // Background thread: only cache the latest frame; conversion happens on capture.
            try
            {
                using var frame = sender.TryAcquireLatestFrame();
                var bmp = frame?.VideoMediaFrame?.SoftwareBitmap;
                if (bmp != null)
                {
                    _latestFrame = bmp;
                }
            }
            catch (Exception)
            {
                // transient frame errors are expected during teardown
            }
        }

        private void Fail(string message)
        {
            _failed = true;
            Busy.IsActive = false;
            Hint.Text = string.Empty;
            Status.Message = message;
            Status.Severity = InfoBarSeverity.Warning;
            Status.IsOpen = true;
            IsPrimaryButtonEnabled = false;
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result != ContentDialogResult.Primary)
            {
                return; // cancel / close: nothing to capture
            }

            if (_failed)
            {
                return;
            }

            var bmp = _latestFrame;
            if (bmp == null)
            {
                args.Cancel = true;
                Status.Message = "No frame has arrived yet — give the preview a moment and try again.";
                Status.Severity = InfoBarSeverity.Informational;
                Status.IsOpen = true;
                return;
            }

            try
            {
                // SoftwareBitmapToPayloadAsync is synchronous under the hood (pixel copy + letterbox).
                Result = ImageOps.SoftwareBitmapToPayloadAsync(bmp, "Camera photo").GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "CameraDialog.Capture");
                args.Cancel = true;
                Fail("Capturing the photo failed. " + ex.Message);
            }
        }

        private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            Teardown();
        }

        private void Teardown()
        {
            try
            {
                if (_reader != null)
                {
                    _reader.FrameArrived -= Reader_FrameArrived;
                    _ = _reader.StopAsync();
                }

                Preview.Source = null;
                _mediaSource?.Dispose();
                _capture?.Dispose();
            }
            catch (Exception ex)
            {
                AppLogger.LogException(ex, "CameraDialog.Teardown");
            }
            finally
            {
                _reader = null;
                _mediaSource = null;
                _capture = null;
                _latestFrame = null;
            }
        }
    }
}
