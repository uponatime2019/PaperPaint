using System;
using System.Threading.Tasks;
using PaperPaint.Models;

namespace PaperPaint.Services
{
    /// <summary>Software pixel effects (BGRA) applied to imported photos.</summary>
    public static class ImageEffects
    {
        public static string DisplayName(PhotoEffect effect) => effect switch
        {
            PhotoEffect.None => "None",
            PhotoEffect.Sketch => "Sketch",
            PhotoEffect.Blur => "Blur",
            PhotoEffect.Sepia => "Sepia",
            PhotoEffect.BlackWhite => "Black & White",
            _ => effect.ToString()
        };

        /// <summary>Runs on a background thread and returns a new processed buffer.</summary>
        public static Task<byte[]> ApplyAsync(PhotoEffect effect, byte[] src, int w, int h)
        {
            return Task.Run(() => Apply(effect, src, w, h));
        }

        public static byte[] Apply(PhotoEffect effect, byte[] src, int w, int h)
        {
            try
            {
                switch (effect)
                {
                    case PhotoEffect.None:
                        return src;
                    case PhotoEffect.Sketch:
                        return Sketch(src, w, h);
                    case PhotoEffect.Blur:
                        return Blur(src, w, h, 6, 2);
                    case PhotoEffect.Sepia:
                        return Sepia(src);
                    case PhotoEffect.BlackWhite:
                        return BlackWhite(src);
                    default:
                        return src;
                }
            }
            catch (Exception)
            {
                return src;
            }
        }

        /// <summary>Warm sepia tone: out = clamp(0.393R + 0.769G + 0.189B, ...) classic matrix.</summary>
        private static byte[] Sepia(byte[] src)
        {
            var dst = (byte[])src.Clone();
            for (int i = 0; i < dst.Length; i += 4)
            {
                double b = dst[i];
                double g = dst[i + 1];
                double r = dst[i + 2];
                double nr = 0.393 * r + 0.769 * g + 0.189 * b;
                double ng = 0.349 * r + 0.686 * g + 0.168 * b;
                double nb = 0.272 * r + 0.534 * g + 0.131 * b;
                dst[i] = Clamp(nb);
                dst[i + 1] = Clamp(ng);
                dst[i + 2] = Clamp(nr);
            }

            return dst;
        }

        /// <summary>Luminance grayscale with a gentle contrast stretch.</summary>
        private static byte[] BlackWhite(byte[] src)
        {
            var dst = (byte[])src.Clone();
            byte min = 255, max = 0;
            var lum = new byte[dst.Length / 4];
            for (int p = 0, i = 0; p < lum.Length; p++, i += 4)
            {
                byte y = (byte)(0.299 * dst[i + 2] + 0.587 * dst[i + 1] + 0.114 * dst[i]);
                lum[p] = y;
                if (y < min) min = y;
                if (y > max) max = y;
            }

            double range = Math.Max(1, max - min);
            for (int p = 0, i = 0; p < lum.Length; p++, i += 4)
            {
                // Stretch to full range, then pull extremes slightly inward for softer whites.
                double v = (lum[p] - min) / range;
                v = 18 + v * 219;
                byte g = Clamp(v);
                dst[i] = g;
                dst[i + 1] = g;
                dst[i + 2] = g;
            }

            return dst;
        }

        /// <summary>Separable box blur (radius r, repeated passes approximate a gaussian).</summary>
        private static byte[] Blur(byte[] src, int w, int h, int radius, int passes)
        {
            var cur = (byte[])src.Clone();
            if (w < 2 * radius + 2 || h < 2 * radius + 2)
            {
                return cur;
            }

            var tmp = new byte[cur.Length];
            for (int p = 0; p < passes; p++)
            {
                BoxPass(cur, tmp, w, h, radius, horizontal: true);
                BoxPass(tmp, cur, w, h, radius, horizontal: false);
            }

            return cur;
        }

        private static void BoxPass(byte[] src, byte[] dst, int w, int h, int r, bool horizontal)
        {
            int outer = horizontal ? h : w;
            int inner = horizontal ? w : h;
            int stride = inner * 4;
            var sums = new int[4];

            for (int o = 0; o < outer; o++)
            {
                int baseIdx = o * stride;
                sums[0] = sums[1] = sums[2] = sums[3] = 0;
                for (int k = -r; k <= r; k++)
                {
                    int idx = baseIdx + Math.Clamp(k, 0, inner - 1) * 4;
                    for (int c = 0; c < 4; c++)
                    {
                        sums[c] += src[idx + c];
                    }
                }

                int window = 2 * r + 1;
                for (int i = 0; i < inner; i++)
                {
                    int di = baseIdx + i * 4;
                    for (int c = 0; c < 4; c++)
                    {
                        dst[di + c] = (byte)(sums[c] / window);
                    }

                    int addIdx = baseIdx + Math.Min(inner - 1, i + r + 1) * 4;
                    int subIdx = baseIdx + Math.Max(0, i - r) * 4;
                    for (int c = 0; c < 4; c++)
                    {
                        sums[c] += src[addIdx + c] - src[subIdx + c];
                    }
                }
            }
        }

        /// <summary>Sobel edge sketch: light paper with dark pencil edges.</summary>
        private static byte[] Sketch(byte[] src, int w, int h)
        {
            int n = w * h;
            var lum = new byte[n];
            for (int p = 0, i = 0; p < n; p++, i += 4)
            {
                lum[p] = (byte)(0.299 * src[i + 2] + 0.587 * src[i + 1] + 0.114 * src[i]);
            }

            var dst = new byte[src.Length];
            ImageOpsFillWhite(dst);
            for (int y = 1; y < h - 1; y++)
            {
                for (int x = 1; x < w - 1; x++)
                {
                    int i = y * w + x;
                    int gx =
                        -lum[i - w - 1] - 2 * lum[i - 1] - lum[i + w - 1] +
                        lum[i - w + 1] + 2 * lum[i + 1] + lum[i + w + 1];
                    int gy =
                        -lum[i - w - 1] - 2 * lum[i - w] - lum[i - w + 1] +
                        lum[i + w - 1] + 2 * lum[i + w] + lum[i + w + 1];
                    int mag = (int)Math.Sqrt((double)gx * gx + gy * gy);

                    // 0 edges -> white paper; strong edges -> dark graphite (floored so the
                    // whole page never goes fully black).
                    double v = 255 - Math.Min(255, mag * 1.35);
                    v = 235 * Math.Pow(v / 255.0, 0.78) + 20;
                    byte g = Clamp(v);
                    int d = i * 4;
                    dst[d] = g;
                    dst[d + 1] = g;
                    dst[d + 2] = g;
                    dst[d + 3] = 255;
                }
            }

            return dst;
        }

        private static void ImageOpsFillWhite(byte[] bgra)
        {
            Helpers.ImageOps.FillWhite(bgra);
        }

        private static byte Clamp(double v) => (byte)Math.Clamp((int)Math.Round(v), 0, 255);
    }
}
