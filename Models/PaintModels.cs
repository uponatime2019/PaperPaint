using System;
using System.Collections.Generic;
using Windows.UI;

namespace PaperPaint.Models
{
    /// <summary>Brush tools available in the paint studio.</summary>
    public enum PaintTool
    {
        Pen,
        Pencil,
        Watercolor,
        Oil,
        Pastel,
        Eraser
    }

    /// <summary>A single sample of a stroke in canvas coordinates (1600x1200 space).</summary>
    public sealed class StrokePoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Pressure { get; set; }

        public StrokePoint(float x, float y, float pressure)
        {
            X = x;
            Y = y;
            Pressure = pressure;
        }
    }

    /// <summary>A deterministic, replayable stroke. All randomness derives from Seed.</summary>
    public sealed class PaintStroke
    {
        public PaintTool Tool { get; set; } = PaintTool.Pen;
        public Color Color { get; set; }
        public float Size { get; set; } = 14f;
        public float Opacity { get; set; } = 1f;
        public int Seed { get; set; }
        public List<StrokePoint> Points { get; } = new List<StrokePoint>();
    }

    /// <summary>Photo effects offered on the Import &amp; Effects screen.</summary>
    public enum PhotoEffect
    {
        None,
        Sketch,
        Blur,
        Sepia,
        BlackWhite
    }

    /// <summary>An opaque BGRA bitmap carried between pages (import / camera / effects).</summary>
    public sealed class ImagePayload
    {
        public byte[] Pixels { get; set; } = Array.Empty<byte>();
        public int Width { get; set; }
        public int Height { get; set; }
        public string? SuggestedName { get; set; }
    }

    /// <summary>Metadata for a locally saved painting. Pure data (JSON serializable).</summary>
    public sealed class ArtworkItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string File { get; set; } = string.Empty;
        public string Thumb { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public DateTime ModifiedUtc { get; set; }
    }

    /// <summary>Payload that tells the studio what canvas to start with.</summary>
    public sealed class StudioLaunch
    {
        public bool NewBlank { get; set; }
        public string? ColoringPageId { get; set; }
        public ImagePayload? BaseImage { get; set; }
        public ArtworkItem? OpenArtwork { get; set; }
    }
}
