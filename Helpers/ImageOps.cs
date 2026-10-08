using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using PaperPaint.Models;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace PaperPaint.Helpers
{
    /// <summary>Bitmap decode / encode / resize helpers shared by import, camera and effects.</summary>
    public static class ImageOps
    {
        /// <summary>Logical painting canvas size. All artwork is composed at this resolution.</summary>
        public const int CanvasWidth = 1600;
        public const int CanvasHeight = 1200;

        /// <summary>Decodes a stream and letterboxes it onto an opaque white 1600x1200 canvas.</summary>
        public static async Task<ImagePayload> DecodeFittedAsync(IRandomAccessStream stream, string? suggestedName = null)
        {
            var decoder = await BitmapDecoder.CreateAsync(stream);
            double fitScale = Math.Min(
                (double)CanvasWidth / decoder.PixelWidth,
                (double)CanvasHeight / decoder.PixelHeight);
            int scaledW = Math.Max(1, (int)Math.Round(decoder.PixelWidth * fitScale));
            int scaledH = Math.Max(1, (int)Math.Round(decoder.PixelHeight * fitScale));

            var transform = new BitmapTransform { ScaledWidth = (uint)scaledW, ScaledHeight = (uint)scaledH };
            var pixels = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform,
                ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
            var src = pixels.DetachPixelData();

            return FitToCanvas(src, scaledW, scaledH, suggestedName);
        }

        /// <summary>Centers an opaque BGRA buffer on a white 1600x1200 buffer.</summary>
        public static ImagePayload FitToCanvas(byte[] src, int srcW, int srcH, string? suggestedName = null)
        {
            var dst = new byte[CanvasWidth * CanvasHeight * 4];
            FillWhite(dst);

            int offX = (CanvasWidth - srcW) / 2;
            int offY = (CanvasHeight - srcH) / 2;
            for (int y = 0; y < srcH; y++)
            {
                int dstRow = ((y + offY) * CanvasWidth + offX) * 4;
                int srcRow = y * srcW * 4;
                for (int x = 0; x < srcW; x++)
                {
                    int d = dstRow + x * 4;
                    int s = srcRow + x * 4;
                    byte a = src[s + 3];
                    if (a == 255)
                    {
                        dst[d] = src[s];
                        dst[d + 1] = src[s + 1];
                        dst[d + 2] = src[s + 2];
                        dst[d + 3] = 255;
                    }
                    else if (a == 0)
                    {
                        // leave white
                    }
                    else
                    {
                        dst[d] = (byte)((src[s] * a + 255 * (255 - a)) / 255);
                        dst[d + 1] = (byte)((src[s + 1] * a + 255 * (255 - a)) / 255);
                        dst[d + 2] = (byte)((src[s + 2] * a + 255 * (255 - a)) / 255);
                        dst[d + 3] = 255;
                    }
                }
            }

            return new ImagePayload { Pixels = dst, Width = CanvasWidth, Height = CanvasHeight, SuggestedName = suggestedName };
        }

        public static void FillWhite(byte[] bgra)
        {
            for (int i = 0; i < bgra.Length; i += 4)
            {
                bgra[i] = 255;
                bgra[i + 1] = 255;
                bgra[i + 2] = 255;
                bgra[i + 3] = 255;
            }
        }

        /// <summary>Bilinear resize of a BGRA buffer (alpha preserved, opaque assumed for painting).</summary>
        public static byte[] ResizeBilinear(byte[] src, int srcW, int srcH, int dstW, int dstH)
        {
            if (srcW == dstW && srcH == dstH)
            {
                return (byte[])src.Clone();
            }

            var dst = new byte[dstW * dstH * 4];
            double xRatio = (double)(srcW - 1) / Math.Max(1, dstW - 1);
            double yRatio = (double)(srcH - 1) / Math.Max(1, dstH - 1);
            for (int y = 0; y < dstH; y++)
            {
                double sy = Math.Min(srcH - 1, y * yRatio);
                int y0 = (int)sy;
                int y1 = Math.Min(srcH - 1, y0 + 1);
                double fy = sy - y0;
                for (int x = 0; x < dstW; x++)
                {
                    double sx = Math.Min(srcW - 1, x * xRatio);
                    int x0 = (int)sx;
                    int x1 = Math.Min(srcW - 1, x0 + 1);
                    double fx = sx - x0;

                    int d = (y * dstW + x) * 4;
                    for (int c = 0; c < 4; c++)
                    {
                        double v00 = src[(y0 * srcW + x0) * 4 + c];
                        double v10 = src[(y0 * srcW + x1) * 4 + c];
                        double v01 = src[(y1 * srcW + x0) * 4 + c];
                        double v11 = src[(y1 * srcW + x1) * 4 + c];
                        double top = v00 + (v10 - v00) * fx;
                        double bot = v01 + (v11 - v01) * fx;
                        dst[d + c] = (byte)(top + (bot - top) * fy);
                    }
                }
            }

            return dst;
        }

        /// <summary>Converts a captured SoftwareBitmap to a fitted ImagePayload.</summary>
        public static Task<ImagePayload> SoftwareBitmapToPayloadAsync(SoftwareBitmap bitmap, string? suggestedName = null)
        {
            SoftwareBitmap bgra = bitmap;
            if (bitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                bitmap.BitmapAlphaMode == BitmapAlphaMode.Premultiplied)
            {
                bgra = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight);
            }

            var bytes = new byte[bgra.PixelWidth * bgra.PixelHeight * 4];
            bgra.CopyToBuffer(bytes.AsBuffer());
            return Task.FromResult(FitToCanvas(bytes, bgra.PixelWidth, bgra.PixelHeight, suggestedName));
        }

        /// <summary>Encodes a BGRA buffer to PNG bytes.</summary>
        public static async Task<byte[]> EncodePngAsync(byte[] pixels, int width, int height)
        {
            using var ms = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, ms);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                (uint)width, (uint)height, 96, 96, pixels);
            await encoder.FlushAsync();
            var size = (int)ms.Size;
            var bytes = new byte[size];
            using var reader = new DataReader(ms.GetInputStreamAt(0));
            await reader.LoadAsync((uint)size);
            reader.ReadBytes(bytes);
            return bytes;
        }
    }
}
