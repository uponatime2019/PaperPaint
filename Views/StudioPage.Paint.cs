using System;
using System.Collections.Generic;
using System.Numerics;
using PaperPaint.Models;
using PaperPaint.Services;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;
using Windows.UI;
using CanvasControl = Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl;

namespace PaperPaint.Views
{
    /// <summary>StudioPage — painting engine: layers, stroke rendering, replay, input.</summary>
    public sealed partial class StudioPage
    {
        private const float CanvasW = Helpers.ImageOps.CanvasWidth;
        private const float CanvasH = Helpers.ImageOps.CanvasHeight;
        private const float MinScale = 0.05f;
        private const float MaxScale = 8f;

        private static readonly Color WhiteColor = Color.FromArgb(255, 255, 255, 255);
        private static readonly Color BlackColor = Color.FromArgb(255, 20, 18, 16);

        private CanvasDevice? _device;
        private CanvasRenderTarget? _paper;
        private CanvasRenderTarget? _baseLayer;   // imported photo or coloring page line art
        private CanvasRenderTarget? _dry;         // committed ("dried") strokes
        private CanvasRenderTarget? _wet;         // strokes since the last Dry Paint
        private CanvasRenderTarget? _mask;        // scratch surface for eraser compositing
        private GaussianBlurEffect? _wetBloom;    // soft bleed that disappears once paint dries

        private readonly CanvasStrokeStyle _roundStyle = new CanvasStrokeStyle
        {
            StartCap = CanvasCapStyle.Round,
            EndCap = CanvasCapStyle.Round,
            DashCap = CanvasCapStyle.Round,
            LineJoin = CanvasLineJoin.Round
        };

        // painting state
        private readonly List<PaintStroke> _strokes = new List<PaintStroke>();
        private readonly List<PaintStroke> _redoStack = new List<PaintStroke>();
        private int _dryIndex;                    // strokes [0.._dryIndex) live in the dry layer
        private PaintStroke? _activeStroke;
        private int _renderedSegments;            // segments of the active stroke already drawn
        private uint _strokePointerId;
        private string? _coloringBaseId;

        // view state
        private float _scale = 1f;
        private Vector2 _offset = Vector2.Zero;
        private bool _panning;
        private Vector2 _panLastScreen;
        private bool _pinching;
        private float _pinchStartDistance;
        private float _pinchStartScale;
        private Vector2 _pinchStartCenter;
        private Vector2 _pinchStartCanvasPoint;
        private readonly Dictionary<uint, Vector2> _touchPoints = new Dictionary<uint, Vector2>();

        private bool HasWetPaint => _strokes.Count > _dryIndex || _activeStroke != null;
        private bool EngineReady => _device != null && _paper != null && _dry != null && _wet != null && _mask != null;

        // ------------------------------------------------------------------
        // resource creation
        // ------------------------------------------------------------------

        private void PaintCanvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            try
            {
                _device = sender.Device;

                _paper?.Dispose();
                _baseLayer?.Dispose();
                _dry?.Dispose();
                _wet?.Dispose();
                _mask?.Dispose();
                _wetBloom?.Dispose();

                _baseLayer = new CanvasRenderTarget(_device, CanvasW, CanvasH, 96);
                _dry = new CanvasRenderTarget(_device, CanvasW, CanvasH, 96);
                _wet = new CanvasRenderTarget(_device, CanvasW, CanvasH, 96);
                _mask = new CanvasRenderTarget(_device, CanvasW, CanvasH, 96);
                _wetBloom = new GaussianBlurEffect { Source = _wet, BlurAmount = 6f, Optimization = EffectOptimization.Balanced };

                BuildPaper(_device);
                RestoreBaseAfterDeviceReset();
                RebuildLayers();
                ApplyPendingLaunchOnCanvas();

                if (_neverFitted)
                {
                    _neverFitted = false;
                    FitPainting();
                }

                UpdateCommandStates();
                sender.Invalidate();
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogException(ex, "PaintCanvas_CreateResources");
            }
        }

        /// <summary>Subtle watercolor-paper tooth, generated once with a fixed seed.</summary>
        private void BuildPaper(CanvasDevice device)
        {
            _paper = new CanvasRenderTarget(device, CanvasW, CanvasH, 96);
            using var ds = _paper.CreateDrawingSession();
            ds.FillRectangle(0, 0, CanvasW, CanvasH, Color.FromArgb(255, 253, 251, 246));

            var rng = new Random(20260820);
            var grain = Color.FromArgb(255, 168, 158, 140);
            for (int i = 0; i < 9000; i++)
            {
                float x = (float)(rng.NextDouble() * CanvasW);
                float y = (float)(rng.NextDouble() * CanvasH);
                byte a = (byte)(3 + rng.Next(7));
                ds.FillRectangle(x, y, 1.4f, 1.4f, Color.FromArgb(a, grain.R, grain.G, grain.B));
            }

            var fiber = Color.FromArgb(255, 150, 150, 145);
            for (int i = 0; i < 500; i++)
            {
                float x = (float)(rng.NextDouble() * CanvasW);
                float y = (float)(rng.NextDouble() * CanvasH);
                double angle = rng.NextDouble() * Math.PI;
                float len = 24 + (float)rng.NextDouble() * 70;
                byte a = (byte)(4 + rng.Next(5));
                ds.DrawLine(x, y,
                    x + (float)Math.Cos(angle) * len,
                    y + (float)Math.Sin(angle) * len,
                    Color.FromArgb(a, fiber.R, fiber.G, fiber.B), 1f);
            }
        }

        private void RestoreBaseAfterDeviceReset()
        {
            // After a device loss the surfaces are recreated empty; repaint the base we know about.
            using (var ds = _baseLayer!.CreateDrawingSession())
            {
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
                if (_coloringBaseId != null)
                {
                    ColoringArt.DrawPage(ds, _coloringBaseId, CanvasW, CanvasH, 8f);
                }
                else if (_baseImagePixels != null)
                {
                    using var bmp = CanvasBitmap.CreateFromBytes(_device!, _baseImagePixels,
                        (int)CanvasW, (int)CanvasH, Windows.Graphics.DirectX.DirectXPixelFormat.B8G8R8A8UIntNormalized, 96, CanvasAlphaMode.Premultiplied);
                    ds.DrawImage(bmp);
                }
            }
        }

        // ------------------------------------------------------------------
        // composite draw
        // ------------------------------------------------------------------

        private void PaintCanvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            try
            {
                if (!EngineReady)
                {
                    return;
                }

                var ds = args.DrawingSession;
                ds.Clear(Color.FromArgb(0, 0, 0, 0));
                ds.Transform = new Matrix3x2(_scale, 0, 0, _scale, _offset.X, _offset.Y);

                // soft plate shadow under the paper
                ds.FillRectangle(-12, -12, CanvasW + 24, CanvasH + 24, Color.FromArgb(30, 62, 48, 26));

                ds.DrawImage(_paper!);
                ds.DrawImage(_baseLayer!);
                ds.DrawImage(_dry!);

                if (HasWetPaint && _wetBloom != null)
                {
                    var full = new Rect(0, 0, CanvasW, CanvasH);
                    ds.DrawImage(_wetBloom, full, full, 0.45f);
                }

                ds.DrawImage(_wet!);
                ds.Transform = Matrix3x2.Identity;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogException(ex, "PaintCanvas_Draw");
            }
        }

        // ------------------------------------------------------------------
        // layer rebuild (undo / redo / dry / clear / load)
        // ------------------------------------------------------------------

        private void RebuildLayers()
        {
            if (!EngineReady)
            {
                return;
            }

            using var dryDs = _dry!.CreateDrawingSession();
            dryDs.Clear(Color.FromArgb(0, 0, 0, 0));

            using var wetDs = _wet!.CreateDrawingSession();
            wetDs.Clear(Color.FromArgb(0, 0, 0, 0));

            for (int i = 0; i < _strokes.Count; i++)
            {
                var s = _strokes[i];
                if (s.Tool == PaintTool.Eraser)
                {
                    DrawStrokeIntoMask(s);
                    ApplyMaskTo(dryDs);
                    ApplyMaskTo(wetDs);
                }
                else
                {
                    RenderStroke(i < _dryIndex ? dryDs : wetDs, s);
                }
            }
        }

        private void DrawStrokeIntoMask(PaintStroke s)
        {
            using var mds = _mask!.CreateDrawingSession();
            mds.Clear(Color.FromArgb(0, 0, 0, 0));
            var white = WhiteColor;
            int segments = Math.Max(1, s.Points.Count - 1);
            for (int i = 0; i < segments; i++)
            {
                var a = s.Points[i];
                var b = s.Points[Math.Min(i + 1, s.Points.Count - 1)];
                float w = EraserWidth(s, i);
                mds.DrawLine(a.X, a.Y, b.X, b.Y, white, w, _roundStyle);
            }
        }

        private void ApplyMaskTo(CanvasDrawingSession target)
        {
            var full = new Rect(0, 0, CanvasW, CanvasH);
            target.DrawImage(_mask!, full, full, 1f, CanvasImageInterpolation.Linear, CanvasComposite.DestinationOut);
        }

        // ------------------------------------------------------------------
        // stroke rendering (deterministic: identical on live draw and replay)
        // ------------------------------------------------------------------

        private void RenderStroke(CanvasDrawingSession ds, PaintStroke s)
        {
            if (s.Points.Count == 0)
            {
                return;
            }

            int segments = Math.Max(1, s.Points.Count - 1);
            for (int i = 0; i < segments; i++)
            {
                RenderSegment(ds, s, i);
            }
        }

        private static Random SegRandom(int seed, int seg)
        {
            return new Random(unchecked(seed * 1000003 + seg * 7919 + 17));
        }

        private static float PressureFactor(float p)
        {
            // Mouse reports a constant 0.5, which maps to 1.0 — pen pressure scales around it.
            return Math.Clamp(p / 0.5f, 0.35f, 1.7f);
        }

        private static float WidthAt(PaintStroke s, int pointIndex)
        {
            var p = s.Points[Math.Min(pointIndex, s.Points.Count - 1)];
            float pf = PressureFactor(p.Pressure);
            return s.Tool switch
            {
                PaintTool.Pen => s.Size * pf,
                PaintTool.Pencil => s.Size * 0.55f * (0.7f + 0.3f * pf),
                PaintTool.Watercolor => s.Size * 2.0f * (0.8f + 0.2f * pf),
                PaintTool.Oil => s.Size * 1.5f * (0.75f + 0.25f * pf),
                PaintTool.Pastel => s.Size * 1.2f * pf,
                PaintTool.Eraser => EraserWidth(s, pointIndex),
                _ => s.Size
            };
        }

        private static float EraserWidth(PaintStroke s, int pointIndex)
        {
            return s.Size * 1.4f;
        }

        private static Color WithAlpha(Color c, float alpha01)
        {
            return Color.FromArgb((byte)Math.Clamp((int)(alpha01 * 255f), 0, 255), c.R, c.G, c.B);
        }

        private void RenderSegment(CanvasDrawingSession ds, PaintStroke s, int i)
        {
            var a = s.Points[i];
            var b = s.Points[Math.Min(i + 1, s.Points.Count - 1)];
            float dx = b.X - a.X;
            float dy = b.Y - a.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            var nrm = dist > 0.001f ? new Vector2(-dy / dist, dx / dist) : new Vector2(0, 1);
            float wa = WidthAt(s, i);
            float wb = WidthAt(s, i + 1);
            var rng = SegRandom(s.Seed, i);
            float op = Math.Clamp(s.Opacity, 0.05f, 1f);
            var c = s.Color;

            switch (s.Tool)
            {
                case PaintTool.Pen:
                {
                    ds.DrawLine(a.X, a.Y, b.X, b.Y, WithAlpha(c, op), (wa + wb) * 0.5f, _roundStyle);
                    break;
                }

                case PaintTool.Pencil:
                {
                    ds.DrawLine(a.X, a.Y, b.X, b.Y, WithAlpha(c, 0.5f * op), (wa + wb) * 0.5f, _roundStyle);
                    int dots = Math.Clamp((int)(dist / 2.2f), 2, 26);
                    for (int k = 0; k < dots; k++)
                    {
                        double t = rng.NextDouble();
                        float jx = a.X + dx * (float)t + nrm.X * ((float)rng.NextDouble() - 0.5f) * wa * 1.1f;
                        float jy = a.Y + dy * (float)t + nrm.Y * ((float)rng.NextDouble() - 0.5f) * wa * 1.1f;
                        float r = wa * (0.05f + (float)rng.NextDouble() * 0.08f);
                        ds.FillCircle(jx, jy, MathF.Max(0.4f, r), WithAlpha(c, (0.18f + (float)rng.NextDouble() * 0.3f) * op));
                    }

                    break;
                }

                case PaintTool.Watercolor:
                {
                    // layered translucent passes with slight jitter build up a wash
                    Vector2 j1 = nrm * ((float)rng.NextDouble() - 0.5f) * wa * 0.22f;
                    Vector2 j2 = nrm * ((float)rng.NextDouble() - 0.5f) * wa * 0.30f;
                    Vector2 j3 = nrm * ((float)rng.NextDouble() - 0.5f) * wa * 0.18f;

                    ds.DrawLine(a.X + j1.X, a.Y + j1.Y, b.X + j1.X, b.Y + j1.Y, WithAlpha(c, 0.05f * op), wa * 1.7f, _roundStyle);
                    ds.DrawLine(a.X + j2.X, a.Y + j2.Y, b.X + j2.X, b.Y + j2.Y, WithAlpha(c, 0.10f * op), wa, _roundStyle);
                    ds.DrawLine(a.X + j3.X, a.Y + j3.Y, b.X + j3.X, b.Y + j3.Y, WithAlpha(c, 0.13f * op), wa * 0.62f, _roundStyle);
                    break;
                }

                case PaintTool.Oil:
                {
                    float bodyW = (wa + wb) * 0.5f;
                    ds.DrawLine(a.X, a.Y, b.X, b.Y, WithAlpha(c, 0.8f * op), bodyW, _roundStyle);

                    // impasto ridge: light catches the top of the stroke, shade below
                    Vector2 off = nrm * bodyW * 0.22f;
                    ds.DrawLine(a.X - off.X, a.Y - off.Y, b.X - off.X, b.Y - off.Y,
                        WithAlpha(WhiteColor, 0.22f * op), bodyW * 0.26f, _roundStyle);
                    ds.DrawLine(a.X + off.X, a.Y + off.Y, b.X + off.X, b.Y + off.Y,
                        WithAlpha(BlackColor, 0.10f * op), bodyW * 0.28f, _roundStyle);
                    break;
                }

                case PaintTool.Pastel:
                {
                    ds.DrawLine(a.X, a.Y, b.X, b.Y, WithAlpha(c, 0.12f * op), wa * 0.8f, _roundStyle);
                    int dabs = Math.Clamp((int)(dist / 1.8f), 3, 40);
                    for (int k = 0; k < dabs; k++)
                    {
                        double t = rng.NextDouble();
                        float px = a.X + dx * (float)t + nrm.X * ((float)rng.NextDouble() - 0.5f) * wa * 0.9f;
                        float py = a.Y + dy * (float)t + nrm.Y * ((float)rng.NextDouble() - 0.5f) * wa * 0.9f;
                        float r = wa * (0.09f + (float)rng.NextDouble() * 0.07f);
                        ds.FillCircle(px, py, MathF.Max(0.5f, r), WithAlpha(c, (0.22f + (float)rng.NextDouble() * 0.3f) * op));
                    }

                    break;
                }

                case PaintTool.Eraser:
                {
                    // Erasers are rendered through the mask path, never here.
                    ds.DrawLine(a.X, a.Y, b.X, b.Y, WhiteColor, wa, _roundStyle);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------
        // live stroke pipeline
        // ------------------------------------------------------------------

        private void BeginStroke(PointerPoint ppt)
        {
            if (!EngineReady)
            {
                return;
            }

            var canvas = ScreenToCanvas(ppt.Position);
            _activeStroke = new PaintStroke
            {
                Tool = _tool,
                Color = _tool == PaintTool.Eraser ? WhiteColor : _currentColor,
                Size = _brushSize,
                Opacity = _brushOpacity,
                Seed = Random.Shared.Next()
            };
            _activeStroke.Points.Add(new StrokePoint(canvas.X, canvas.Y, PressureOf(ppt)));
            _renderedSegments = 0;
            _strokePointerId = ppt.PointerId;
            _redoStack.Clear();

            // dot for a simple tap
            if (_tool == PaintTool.Eraser)
            {
                DrawLiveEraserSegments();
            }
            else
            {
                DrawLiveSegments();
            }

            UpdateCommandStates();
        }

        private void MoveStroke(PointerPoint ppt)
        {
            if (_activeStroke == null || ppt.PointerId != _strokePointerId)
            {
                return;
            }

            var canvas = ScreenToCanvas(ppt.Position);
            var pts = _activeStroke.Points;
            var last = pts[^1];
            float dx = canvas.X - last.X;
            float dy = canvas.Y - last.Y;
            if (MathF.Sqrt(dx * dx + dy * dy) < 1.2f)
            {
                return;
            }

            pts.Add(new StrokePoint(canvas.X, canvas.Y, PressureOf(ppt)));

            if (_activeStroke.Tool == PaintTool.Eraser)
            {
                DrawLiveEraserSegments();
            }
            else
            {
                DrawLiveSegments();
            }
        }

        private void EndStroke()
        {
            if (_activeStroke == null)
            {
                return;
            }

            if (_activeStroke.Points.Count == 1)
            {
                var p = _activeStroke.Points[0];
                _activeStroke.Points.Add(new StrokePoint(p.X + 0.5f, p.Y + 0.5f, p.Pressure));
                if (_activeStroke.Tool == PaintTool.Eraser)
                {
                    DrawLiveEraserSegments();
                }
                else
                {
                    DrawLiveSegments();
                }
            }

            _strokes.Add(_activeStroke);
            _activeStroke = null;
            _dirty = true;
            UpdateCommandStates();
            PaintCanvas.Invalidate();
        }

        private void DrawLiveSegments()
        {
            if (_activeStroke == null)
            {
                return;
            }

            using var wds = _wet!.CreateDrawingSession();
            int segments = _activeStroke.Points.Count - 1;
            for (int i = _renderedSegments; i < segments; i++)
            {
                RenderSegment(wds, _activeStroke, i);
            }

            _renderedSegments = Math.Max(_renderedSegments, segments);
            PaintCanvas.Invalidate();
        }

        private void DrawLiveEraserSegments()
        {
            if (_activeStroke == null)
            {
                return;
            }

            using (var mds = _mask!.CreateDrawingSession())
            {
                mds.Clear(Color.FromArgb(0, 0, 0, 0));
                int segments = _activeStroke.Points.Count - 1;
                for (int i = 0; i < segments; i++)
                {
                    var a = _activeStroke.Points[i];
                    var b = _activeStroke.Points[i + 1];
                    mds.DrawLine(a.X, a.Y, b.X, b.Y, WhiteColor, EraserWidth(_activeStroke, i), _roundStyle);
                }
            }

            using (var dds = _dry!.CreateDrawingSession())
            {
                ApplyMaskTo(dds);
            }

            using (var wds = _wet!.CreateDrawingSession())
            {
                ApplyMaskTo(wds);
            }

            _renderedSegments = Math.Max(_renderedSegments, _activeStroke.Points.Count - 1);
            PaintCanvas.Invalidate();
        }

        private static float PressureOf(PointerPoint ppt)
        {
            float p = ppt.Properties.Pressure;
            return p > 0f ? p : 0.5f;
        }

        // ------------------------------------------------------------------
        // history: undo / redo / dry / clear
        // ------------------------------------------------------------------

        private void Undo()
        {
            if (_activeStroke != null || _strokes.Count == 0)
            {
                return;
            }

            _redoStack.Add(_strokes[^1]);
            _strokes.RemoveAt(_strokes.Count - 1);
            if (_dryIndex > _strokes.Count)
            {
                _dryIndex = _strokes.Count;
            }

            RebuildLayers();
            _dirty = true;
            UpdateCommandStates();
            PaintCanvas.Invalidate();
        }

        private void Redo()
        {
            if (_redoStack.Count == 0)
            {
                return;
            }

            var s = _redoStack[^1];
            _redoStack.RemoveAt(_redoStack.Count - 1);
            _strokes.Add(s);
            RebuildLayers();
            _dirty = true;
            UpdateCommandStates();
            PaintCanvas.Invalidate();
        }

        private void DryPaint()
        {
            if (!HasWetPaint)
            {
                return;
            }

            EndStroke();
            _dryIndex = _strokes.Count;
            RebuildLayers();
            UpdateCommandStates();
            PaintCanvas.Invalidate();
        }

        private void ClearPainting()
        {
            _activeStroke = null;
            _strokes.Clear();
            _redoStack.Clear();
            _dryIndex = 0;
            RebuildLayers();
            _dirty = true;
            UpdateCommandStates();
            PaintCanvas.Invalidate();
        }

        // ------------------------------------------------------------------
        // view: zoom & pan
        // ------------------------------------------------------------------

        private Vector2 ScreenToCanvas(Point screen)
        {
            return new Vector2(
                ((float)screen.X - _offset.X) / _scale,
                ((float)screen.Y - _offset.Y) / _scale);
        }

        private void FitPainting()
        {
            var size = PaintCanvas.ActualSize;
            if (size.X < 10 || size.Y < 10)
            {
                return;
            }

            const float margin = 40f;
            float s = MathF.Min((size.X - margin * 2) / CanvasW, (size.Y - margin * 2) / CanvasH);
            s = Math.Clamp(s, MinScale, MaxScale);
            _scale = s;
            _offset = new Vector2((size.X - CanvasW * s) / 2f, (size.Y - CanvasH * s) / 2f);
            UpdateZoomText();
            PaintCanvas.Invalidate();
        }

        private void ZoomAt(Vector2 screenPos, float factor)
        {
            var canvasPt = ScreenToCanvas(new Point(screenPos.X, screenPos.Y));
            _scale = Math.Clamp(_scale * factor, MinScale, MaxScale);
            _offset = screenPos - canvasPt * _scale;
            UpdateZoomText();
            PaintCanvas.Invalidate();
        }

        private void ZoomStep(float factor)
        {
            var size = PaintCanvas.ActualSize;
            ZoomAt(new Vector2(size.X / 2f, size.Y / 2f), factor);
        }

        private void ResetZoom()
        {
            var size = PaintCanvas.ActualSize;
            var canvasCenter = new Vector2(CanvasW / 2f, CanvasH / 2f);
            _scale = 1f;
            _offset = new Vector2(size.X / 2f, size.Y / 2f) - canvasCenter;
            UpdateZoomText();
            PaintCanvas.Invalidate();
        }

        // ------------------------------------------------------------------
        // pointer input
        // ------------------------------------------------------------------

        private void PaintCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var ppt = e.GetCurrentPoint(PaintCanvas);
                var screen = new Vector2((float)ppt.Position.X, (float)ppt.Position.Y);
                var props = ppt.Properties;
                var kind = e.Pointer.PointerDeviceType;

                if (kind == PointerDeviceType.Touch)
                {
                    _touchPoints[e.Pointer.PointerId] = screen;
                    if (_touchPoints.Count >= 2)
                    {
                        EndStroke();
                        BeginPinch();
                    }
                    else if (!_pinching)
                    {
                        BeginStroke(ppt);
                    }
                }
                else if (props.IsMiddleButtonPressed || props.IsRightButtonPressed || props.IsBarrelButtonPressed)
                {
                    _panning = true;
                    _panLastScreen = screen;
                }
                else
                {
                    BeginStroke(ppt);
                }

                PaintCanvas.CapturePointer(e.Pointer);
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogException(ex, "PaintCanvas_PointerPressed");
            }
        }

        private void PaintCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var ppt = e.GetCurrentPoint(PaintCanvas);
                var screen = new Vector2((float)ppt.Position.X, (float)ppt.Position.Y);

                if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch && _touchPoints.ContainsKey(e.Pointer.PointerId))
                {
                    _touchPoints[e.Pointer.PointerId] = screen;
                    if (_pinching)
                    {
                        UpdatePinch();
                        e.Handled = true;
                        return;
                    }
                }

                if (_pinching)
                {
                    return;
                }

                if (_panning)
                {
                    _offset += screen - _panLastScreen;
                    _panLastScreen = screen;
                    PaintCanvas.Invalidate();
                    e.Handled = true;
                    return;
                }

                if (ppt.IsInContact || e.Pointer.PointerDeviceType == PointerDeviceType.Pen)
                {
                    MoveStroke(ppt);
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogException(ex, "PaintCanvas_PointerMoved");
            }
        }

        private void PaintCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                if (e.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                {
                    _touchPoints.Remove(e.Pointer.PointerId);
                    if (_touchPoints.Count < 2)
                    {
                        _pinching = false;
                    }
                }

                EndStroke();
                _panning = false;
                try
                {
                    PaintCanvas.ReleasePointerCapture(e.Pointer);
                }
                catch (Exception)
                {
                    // capture may already be gone
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogException(ex, "PaintCanvas_PointerReleased");
            }
        }

        private void PaintCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var ppt = e.GetCurrentPoint(PaintCanvas);
                int delta = ppt.Properties.MouseWheelDelta;
                if (delta != 0)
                {
                    ZoomAt(new Vector2((float)ppt.Position.X, (float)ppt.Position.Y), delta > 0 ? 1.12f : 1f / 1.12f);
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                Helpers.AppLogger.LogException(ex, "PaintCanvas_PointerWheelChanged");
            }
        }

        private void BeginPinch()
        {
            if (_touchPoints.Count < 2)
            {
                return;
            }

            var ids = new List<uint>(_touchPoints.Keys);
            var p1 = _touchPoints[ids[0]];
            var p2 = _touchPoints[ids[1]];
            _pinchStartDistance = MathF.Max(1f, Vector2.Distance(p1, p2));
            _pinchStartCenter = (p1 + p2) / 2f;
            _pinchStartScale = _scale;
            _pinchStartCanvasPoint = (_pinchStartCenter - _offset) / _scale;
            _pinching = true;
        }

        private void UpdatePinch()
        {
            if (!_pinching || _touchPoints.Count < 2)
            {
                return;
            }

            var ids = new List<uint>(_touchPoints.Keys);
            var p1 = _touchPoints[ids[0]];
            var p2 = _touchPoints[ids[1]];
            float dist = MathF.Max(1f, Vector2.Distance(p1, p2));
            var center = (p1 + p2) / 2f;

            _scale = Math.Clamp(_pinchStartScale * (dist / _pinchStartDistance), MinScale, MaxScale);
            _offset = center - _pinchStartCanvasPoint * _scale;
            UpdateZoomText();
            PaintCanvas.Invalidate();
        }
    }
}
