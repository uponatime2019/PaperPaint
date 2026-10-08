using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.UI;

namespace PaperPaint.Services
{
    /// <summary>A built-in coloring page: id, title and category.</summary>
    public sealed class ColoringPageDef
    {
        public string Id { get; }
        public string Title { get; }
        public string Category { get; }

        public ColoringPageDef(string id, string title, string category)
        {
            Id = id;
            Title = title;
            Category = category;
        }
    }

    /// <summary>
    /// Original vector line-art for the built-in coloring gallery. All pages are drawn
    /// procedurally in an 800x600 design space and scaled to any target size.
    /// </summary>
    public static class ColoringArt
    {
        public const float DesignWidth = 800f;
        public const float DesignHeight = 600f;

        private static readonly Color Paper = Color.FromArgb(255, 255, 255, 255);
        private static readonly Color Ink = Color.FromArgb(255, 38, 34, 30);

        public static readonly IReadOnlyList<ColoringPageDef> Pages = new ColoringPageDef[]
        {
            new ColoringPageDef("flower", "Flower Bloom", "Nature"),
            new ColoringPageDef("tree", "Shady Tree", "Nature"),
            new ColoringPageDef("leafvine", "Leaf & Vine", "Nature"),
            new ColoringPageDef("sunclouds", "Sun & Clouds", "Nature"),
            new ColoringPageDef("cat", "Curious Cat", "Animals"),
            new ColoringPageDef("fish", "Happy Fish", "Animals"),
            new ColoringPageDef("butterfly", "Butterfly", "Animals"),
            new ColoringPageDef("snail", "Garden Snail", "Animals"),
            new ColoringPageDef("boat", "Sailboat", "Things"),
            new ColoringPageDef("house", "Cozy House", "Things"),
            new ColoringPageDef("kite", "Flying Kite", "Things"),
            new ColoringPageDef("cupcake", "Sweet Cupcake", "Things"),
            new ColoringPageDef("forest", "Forest Scene", "Scenes"),
            new ColoringPageDef("undersea", "Under the Sea", "Scenes"),
            // ---- complex pages ----
            new ColoringPageDef("rose", "Rose Garden", "Nature"),
            new ColoringPageDef("mushrooms", "Mushroom Grove", "Nature"),
            new ColoringPageDef("lotus", "Lotus Pond", "Nature"),
            new ColoringPageDef("oak", "Mighty Oak", "Nature"),
            new ColoringPageDef("owl", "Midnight Owl", "Animals"),
            new ColoringPageDef("peacock", "Proud Peacock", "Animals"),
            new ColoringPageDef("turtle", "Sea Turtle Reef", "Animals"),
            new ColoringPageDef("elephant", "Decorated Elephant", "Animals"),
            new ColoringPageDef("fox", "Woodland Fox", "Animals"),
            new ColoringPageDef("octopus", "Giant Octopus", "Animals"),
            new ColoringPageDef("lion", "Majestic Lion", "Animals"),
            new ColoringPageDef("castle", "Enchanted Castle", "Fantasy"),
            new ColoringPageDef("dragon", "Friendly Dragon", "Fantasy"),
            new ColoringPageDef("unicorn", "Mystic Unicorn", "Fantasy"),
            new ColoringPageDef("mermaid", "Mermaid Cove", "Fantasy"),
            new ColoringPageDef("fairyhouse", "Fairy Cottage", "Fantasy"),
            new ColoringPageDef("balloons", "Balloon Festival", "Things"),
            new ColoringPageDef("dreamcatcher", "Dreamcatcher", "Things"),
            new ColoringPageDef("ferris", "Grand Ferris Wheel", "Things"),
            new ColoringPageDef("lighthouse", "Lighthouse Cove", "Scenes"),
            new ColoringPageDef("village", "Alpine Village", "Scenes"),
            new ColoringPageDef("space", "Cosmic Voyage", "Scenes"),
            new ColoringPageDef("citynight", "City Lights", "Scenes"),
            new ColoringPageDef("mandalasun", "Solar Mandala", "Mandalas"),
            new ColoringPageDef("mandalatide", "Tidal Mandala", "Mandalas"),
            new ColoringPageDef("mandalastar", "Celestial Mandala", "Mandalas"),
        };

        public static IReadOnlyList<string> Categories { get; } = new string[] { "Nature", "Animals", "Things", "Scenes", "Fantasy", "Mandalas" };

        public static ColoringPageDef? Find(string id)
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                if (Pages[i].Id == id)
                {
                    return Pages[i];
                }
            }

            return null;
        }

        /// <summary>Draws a page onto any target size: white paper, then scaled line art.</summary>
        public static void DrawPage(CanvasDrawingSession ds, string id, float width, float height, float strokeWidth = 7f)
        {
            ds.FillRectangle(0, 0, width, height, Paper);

            float scale = Math.Min(width / DesignWidth, height / DesignHeight);
            float ox = (width - DesignWidth * scale) / 2f;
            float oy = (height - DesignHeight * scale) / 2f;

            var prev = ds.Transform;
            ds.Transform = new Matrix3x2(scale, 0, 0, scale, ox, oy);

            try
            {
                switch (id)
                {
                    case "flower": DrawFlower(ds, strokeWidth); break;
                    case "tree": DrawTree(ds, strokeWidth); break;
                    case "leafvine": DrawLeafVine(ds, strokeWidth); break;
                    case "sunclouds": DrawSunClouds(ds, strokeWidth); break;
                    case "cat": DrawCat(ds, strokeWidth); break;
                    case "fish": DrawFish(ds, strokeWidth); break;
                    case "butterfly": DrawButterfly(ds, strokeWidth); break;
                    case "snail": DrawSnail(ds, strokeWidth); break;
                    case "boat": DrawBoat(ds, strokeWidth); break;
                    case "house": DrawHouse(ds, strokeWidth); break;
                    case "kite": DrawKite(ds, strokeWidth); break;
                    case "cupcake": DrawCupcake(ds, strokeWidth); break;
                    case "forest": DrawForest(ds, strokeWidth); break;
                    case "undersea": DrawUndersea(ds, strokeWidth); break;
                    case "rose": DrawRose(ds, strokeWidth); break;
                    case "mushrooms": DrawMushrooms(ds, strokeWidth); break;
                    case "lotus": DrawLotus(ds, strokeWidth); break;
                    case "oak": DrawOak(ds, strokeWidth); break;
                    case "owl": DrawOwl(ds, strokeWidth); break;
                    case "peacock": DrawPeacock(ds, strokeWidth); break;
                    case "turtle": DrawSeaTurtle(ds, strokeWidth); break;
                    case "elephant": DrawElephant(ds, strokeWidth); break;
                    case "fox": DrawFox(ds, strokeWidth); break;
                    case "octopus": DrawOctopus(ds, strokeWidth); break;
                    case "lion": DrawLion(ds, strokeWidth); break;
                    case "castle": DrawCastle(ds, strokeWidth); break;
                    case "dragon": DrawDragon(ds, strokeWidth); break;
                    case "unicorn": DrawUnicorn(ds, strokeWidth); break;
                    case "mermaid": DrawMermaid(ds, strokeWidth); break;
                    case "fairyhouse": DrawFairyHouse(ds, strokeWidth); break;
                    case "balloons": DrawBalloons(ds, strokeWidth); break;
                    case "dreamcatcher": DrawDreamcatcher(ds, strokeWidth); break;
                    case "ferris": DrawFerris(ds, strokeWidth); break;
                    case "lighthouse": DrawLighthouse(ds, strokeWidth); break;
                    case "village": DrawVillage(ds, strokeWidth); break;
                    case "space": DrawSpace(ds, strokeWidth); break;
                    case "citynight": DrawCityNight(ds, strokeWidth); break;
                    case "mandalasun": DrawMandalaSun(ds, strokeWidth); break;
                    case "mandalatide": DrawMandalaTide(ds, strokeWidth); break;
                    case "mandalastar": DrawMandalaStar(ds, strokeWidth); break;
                }
            }
            finally
            {
                ds.Transform = prev;
            }
        }

        // ----- geometry helpers -------------------------------------------------

        private static CanvasGeometry Ellipse(ICanvasResourceCreator rc, float cx, float cy, float rx, float ry)
        {
            return CanvasGeometry.CreateEllipse(rc, cx, cy, rx, ry);
        }

        private static CanvasGeometry Circle(ICanvasResourceCreator rc, float cx, float cy, float r)
        {
            return CanvasGeometry.CreateEllipse(rc, cx, cy, r, r);
        }

        private static CanvasGeometry RoundedRect(ICanvasResourceCreator rc, float x, float y, float w, float h, float r)
        {
            return CanvasGeometry.CreateRoundedRectangle(rc, x, y, w, h, r, r);
        }

        private static CanvasGeometry Poly(ICanvasResourceCreator rc, params Vector2[] pts)
        {
            var builder = new CanvasPathBuilder(rc);
            builder.BeginFigure(pts[0]);
            for (int i = 1; i < pts.Length; i++)
            {
                builder.AddLine(pts[i]);
            }

            builder.EndFigure(CanvasFigureLoop.Open);
            return CanvasGeometry.CreatePath(builder);
        }

        private static CanvasGeometry ClosedPoly(ICanvasResourceCreator rc, params Vector2[] pts)
        {
            var builder = new CanvasPathBuilder(rc);
            builder.BeginFigure(pts[0]);
            for (int i = 1; i < pts.Length; i++)
            {
                builder.AddLine(pts[i]);
            }

            builder.EndFigure(CanvasFigureLoop.Closed);
            return CanvasGeometry.CreatePath(builder);
        }

        private static CanvasGeometry Path(ICanvasResourceCreator rc, Action<CanvasPathBuilder> build, bool close = false)
        {
            var builder = new CanvasPathBuilder(rc);
            build(builder);
            builder.EndFigure(close ? CanvasFigureLoop.Closed : CanvasFigureLoop.Open);
            return CanvasGeometry.CreatePath(builder);
        }

        private static CanvasGeometry Rotated(CanvasGeometry g, float degrees, Vector2 center)
        {
            return g.Transform(Matrix3x2.CreateRotation(degrees * (float)Math.PI / 180f, center));
        }

        private static void Stroke(CanvasDrawingSession ds, CanvasGeometry g, float w)
        {
            ds.DrawGeometry(g, Ink, w);
        }

        private static void Line(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, float w)
        {
            ds.DrawLine(x1, y1, x2, y2, Ink, w);
        }

        private static void Dot(CanvasDrawingSession ds, float cx, float cy, float r)
        {
            ds.FillCircle(cx, cy, r, Ink);
        }

        /// <summary>A pointed leaf built from two quadratic curves meeting at the tip.</summary>
        private static CanvasGeometry Leaf(ICanvasResourceCreator rc, Vector2 basePt, Vector2 tip, float bow)
        {
            return Path(rc, b =>
            {
                b.BeginFigure(basePt);
                var mid = (basePt + tip) / 2f;
                var n = new Vector2(-(tip.Y - basePt.Y), tip.X - basePt.X);
                n = Vector2.Normalize(n) * bow;
                b.AddQuadraticBezier(mid + n, tip);
                b.AddQuadraticBezier(mid - n, basePt);
            }, close: true);
        }

        // ----- pages ------------------------------------------------------------

        private static void DrawFlower(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var center = new Vector2(400, 250);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                var petalCenter = center + Vector2.Transform(new Vector2(0, -85), Matrix3x2.CreateRotation(angle * (float)Math.PI / 180f));
                var petal = Rotated(Ellipse(rc, 400, 165, 72, 34), angle, petalCenter);
                Stroke(ds, petal, w);
            }

            Stroke(ds, Circle(rc, 400, 250, 48), w);

            // stem and leaves
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 298);
                b.AddQuadraticBezier(new Vector2(385, 420), new Vector2(400, 545));
            }), w);

            Stroke(ds, Rotated(Leaf(rc, new Vector2(398, 450), new Vector2(300, 415), 28), 0, Vector2.Zero), w);
            Stroke(ds, Rotated(Leaf(rc, new Vector2(402, 490), new Vector2(505, 455), 28), 0, Vector2.Zero), w);

            // ground
            Stroke(ds, Poly(rc,
                new Vector2(180, 545), new Vector2(280, 552), new Vector2(390, 545),
                new Vector2(500, 552), new Vector2(620, 545)), w);
        }

        private static void DrawTree(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // trunk
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(372, 360);
                b.AddLine(380, 540);
                b.AddLine(420, 540);
                b.AddLine(428, 360);
            }), w);
            Line(ds, 380, 430, 420, 430, w * 0.7f);

            // crown
            Stroke(ds, Circle(rc, 400, 240, 128), w);
            Stroke(ds, Circle(rc, 295, 300, 86), w);
            Stroke(ds, Circle(rc, 505, 300, 86), w);

            // grass
            Line(ds, 160, 540, 640, 540, w);
            for (int i = 0; i < 6; i++)
            {
                float gx = 190 + i * 84;
                Stroke(ds, Poly(rc, new Vector2(gx, 540), new Vector2(gx + 8, 518)), w * 0.7f);
            }
        }

        private static void DrawLeafVine(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(90, 90);
                b.AddCubicBezier(new Vector2(300, 60), new Vector2(280, 330), new Vector2(460, 330));
                b.AddCubicBezier(new Vector2(640, 330), new Vector2(600, 540), new Vector2(730, 530));
            }), w);

            Vector2[] anchors =
            {
                new(180, 96), new(285, 210), new(395, 315), new(505, 345), new(600, 445), new(690, 515),
            };
            for (int i = 0; i < anchors.Length; i++)
            {
                var a = anchors[i];
                var dir = (i % 2 == 0) ? new Vector2(0.55f, -0.84f) : new Vector2(0.5f, 0.86f);
                var tip = a + dir * 120;
                Stroke(ds, Leaf(rc, a, tip, 34), w);
                Line(ds, a.X, a.Y, (a.X + tip.X) / 2 + dir.Y * 8, (a.Y + tip.Y) / 2 - dir.X * 8, w * 0.5f);
            }
        }

        private static void DrawSunClouds(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // sun with rays
            Stroke(ds, Circle(rc, 190, 170, 72), w);
            for (int i = 0; i < 10; i++)
            {
                float a = i * 36f * (float)Math.PI / 180f;
                var dir = new Vector2((float)Math.Sin(a), -(float)Math.Cos(a));
                var p1 = new Vector2(190, 170) + dir * 88;
                var p2 = new Vector2(190, 170) + dir * 118;
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w);
            }

            // smiling sun
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(162, 155);
                b.AddQuadraticBezier(new Vector2(190, 178), new Vector2(218, 155));
            }), w * 0.7f);
            Dot(ds, 168, 145, 5);
            Dot(ds, 212, 145, 5);

            // two clouds
            Stroke(ds, Circle(rc, 480, 190, 48), w);
            Stroke(ds, Circle(rc, 545, 165, 60), w);
            Stroke(ds, Circle(rc, 612, 195, 44), w);
            Line(ds, 432, 210, 656, 210, w);

            Stroke(ds, Circle(rc, 320, 330, 38), w);
            Stroke(ds, Circle(rc, 375, 312, 46), w);
            Stroke(ds, Circle(rc, 426, 335, 34), w);
            Line(ds, 282, 348, 460, 348, w);

            // rolling hill
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(80, 520);
                b.AddQuadraticBezier(new Vector2(400, 400), new Vector2(720, 520));
            }), w);
        }

        private static void DrawCat(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // body
            Stroke(ds, Ellipse(rc, 400, 435, 155, 115), w);

            // head
            Stroke(ds, Circle(rc, 400, 255, 108), w);

            // ears
            Stroke(ds, ClosedPoly(rc,
                new Vector2(322, 190), new Vector2(300, 78), new Vector2(392, 158)), w);
            Stroke(ds, ClosedPoly(rc,
                new Vector2(478, 190), new Vector2(500, 78), new Vector2(408, 158)), w);

            // face
            Dot(ds, 366, 238, 8);
            Dot(ds, 434, 238, 8);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(390, 268);
                b.AddLine(410, 268);
                b.AddLine(400, 282);
            }, close: true), w * 0.7f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 282);
                b.AddQuadraticBezier(new Vector2(398, 298), new Vector2(382, 300));
            }), w * 0.7f);

            for (int i = 0; i < 3; i++)
            {
                Line(ds, 318, 240 + i * 18, 232, 228 + i * 24, w * 0.55f);
                Line(ds, 482, 240 + i * 18, 568, 228 + i * 24, w * 0.55f);
            }

            // tail
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(548, 470);
                b.AddCubicBezier(new Vector2(680, 470), new Vector2(672, 300), new Vector2(560, 288));
            }), w);

            // paws
            Stroke(ds, Circle(rc, 330, 540, 24), w);
            Stroke(ds, Circle(rc, 470, 540, 24), w);
        }

        private static void DrawFish(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // body lens
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(250, 300);
                b.AddQuadraticBezier(new Vector2(400, 155), new Vector2(550, 300));
                b.AddQuadraticBezier(new Vector2(400, 445), new Vector2(250, 300));
            }, close: true), w);

            // tail
            Stroke(ds, ClosedPoly(rc,
                new Vector2(548, 300), new Vector2(660, 220), new Vector2(628, 300), new Vector2(660, 380)), w);

            // top fin
            Stroke(ds, ClosedPoly(rc, new Vector2(390, 196), new Vector2(430, 120), new Vector2(470, 210)), w);
            // bottom fin
            Stroke(ds, ClosedPoly(rc, new Vector2(390, 404), new Vector2(420, 470), new Vector2(460, 398)), w);

            // gill + eye + mouth
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(330, 230);
                b.AddQuadraticBezier(new Vector2(300, 300), new Vector2(330, 370));
            }), w);
            Dot(ds, 305, 268, 11);
            Stroke(ds, Circle(rc, 305, 268, 22), w * 0.6f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(254, 322);
                b.AddQuadraticBezier(new Vector2(280, 336), new Vector2(300, 326));
            }), w * 0.6f);

            // scales
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Stroke(ds, Path(rc, b =>
                    {
                        var p = new Vector2(400 + c * 48, 270 + r * 58);
                        b.BeginFigure(p);
                        b.AddQuadraticBezier(p + new Vector2(22, -4), p + new Vector2(44, 10));
                    }), w * 0.5f);
                }
            }

            // bubbles
            Stroke(ds, Circle(rc, 180, 200, 22), w);
            Stroke(ds, Circle(rc, 140, 140, 32), w);
            Stroke(ds, Circle(rc, 205, 88, 16), w);
        }

        private static void DrawButterfly(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // body + head
            Stroke(ds, Ellipse(rc, 400, 310, 17, 92), w);
            Stroke(ds, Circle(rc, 400, 198, 20), w);

            // antennae
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(390, 182);
                b.AddQuadraticBezier(new Vector2(358, 130), new Vector2(330, 118));
            }), w * 0.7f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(410, 182);
                b.AddQuadraticBezier(new Vector2(442, 130), new Vector2(470, 118));
            }), w * 0.7f);
            Dot(ds, 330, 118, 7);
            Dot(ds, 470, 118, 7);

            // upper wings
            Stroke(ds, Rotated(Ellipse(rc, 290, 252, 112, 84), -18, new Vector2(290, 252)), w);
            Stroke(ds, Rotated(Ellipse(rc, 510, 252, 112, 84), 18, new Vector2(510, 252)), w);

            // lower wings
            Stroke(ds, Rotated(Ellipse(rc, 318, 398, 80, 62), 14, new Vector2(318, 398)), w);
            Stroke(ds, Rotated(Ellipse(rc, 482, 398, 80, 62), -14, new Vector2(482, 398)), w);

            // wing spots
            Stroke(ds, Circle(rc, 278, 240, 26), w * 0.6f);
            Stroke(ds, Circle(rc, 522, 240, 26), w * 0.6f);
            Dot(ds, 322, 396, 12);
            Dot(ds, 478, 396, 12);
        }

        private static void DrawSnail(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // shell spiral
            var spiral = new List<Vector2>();
            for (int i = 0; i <= 220; i++)
            {
                double t = i / 220.0 * 4.0 * Math.PI;
                float r = 6f + (float)t * 21.5f;
                if (r > 130) break;
                spiral.Add(new Vector2(500 + r * (float)Math.Cos(t), 330 + r * (float)Math.Sin(t)));
            }

            Stroke(ds, Poly(rc, spiral.ToArray()), w);
            Stroke(ds, Circle(rc, 500, 330, 136), w);

            // body
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(200, 430);
                b.AddQuadraticBezier(new Vector2(280, 368), new Vector2(370, 395));
                b.AddLine(636, 395);
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(200, 430);
                b.AddLine(660, 430);
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(200, 430);
                b.AddQuadraticBezier(new Vector2(160, 400), new Vector2(178, 352));
            }), w);

            // antennae
            Line(ds, 186, 352, 150, 275, w);
            Line(ds, 205, 348, 210, 262, w);
            Dot(ds, 150, 272, 9);
            Dot(ds, 211, 259, 9);

            // ground
            Line(ds, 120, 470, 700, 470, w);
        }

        private static void DrawBoat(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // sun
            Stroke(ds, Circle(rc, 685, 95, 42), w);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * (float)Math.PI / 180f;
                var dir = new Vector2((float)Math.Sin(a), -(float)Math.Cos(a));
                var p1 = new Vector2(685, 95) + dir * 54;
                var p2 = new Vector2(685, 95) + dir * 76;
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w);
            }

            // mast + sails
            Line(ds, 400, 430, 400, 150, w);
            Stroke(ds, ClosedPoly(rc, new Vector2(415, 175), new Vector2(415, 415), new Vector2(580, 415)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(385, 205), new Vector2(385, 415), new Vector2(255, 415)), w);

            // flag
            Stroke(ds, ClosedPoly(rc, new Vector2(400, 150), new Vector2(452, 163), new Vector2(400, 176)), w);

            // hull
            Stroke(ds, ClosedPoly(rc,
                new Vector2(220, 435), new Vector2(580, 435), new Vector2(525, 505), new Vector2(275, 505)), w);

            // waves
            for (int row = 0; row < 2; row++)
            {
                float y = 540 + row * 34;
                var pts = new List<Vector2> { new(90, y) };
                for (int i = 0; i < 8; i++)
                {
                    float x0 = 110 + i * 80;
                    pts.Add(new Vector2(x0, y - (i % 2 == 0 ? 14 : 0)));
                    pts.Add(new Vector2(x0 + 40, y));
                }

                Stroke(ds, Poly(rc, pts.ToArray()), w * 0.8f);
            }
        }

        private static void DrawHouse(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // base + roof
            Stroke(ds, RoundedRect(rc, 250, 320, 300, 200, 4), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(225, 320), new Vector2(400, 175), new Vector2(575, 320)), w);

            // chimney
            Stroke(ds, RoundedRect(rc, 495, 205, 38, 72, 4), w);

            // door
            Stroke(ds, RoundedRect(rc, 368, 408, 64, 112, 8), w);
            Dot(ds, 390, 466, 5);

            // windows
            Stroke(ds, RoundedRect(rc, 282, 356, 62, 62, 4), w);
            Stroke(ds, RoundedRect(rc, 456, 356, 62, 62, 4), w);
            Line(ds, 313, 356, 313, 418, w * 0.6f);
            Line(ds, 282, 387, 344, 387, w * 0.6f);
            Line(ds, 487, 356, 487, 418, w * 0.6f);
            Line(ds, 456, 387, 518, 387, w * 0.6f);

            // tree
            Stroke(ds, Circle(rc, 675, 385, 58), w);
            Line(ds, 675, 443, 675, 522, w);

            // ground + path
            Line(ds, 140, 522, 760, 522, w);
            Stroke(ds, Poly(rc, new Vector2(382, 520), new Vector2(360, 560), new Vector2(400, 560), new Vector2(418, 520)), w * 0.6f);
        }

        private static void DrawKite(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // kite diamond + cross
            Stroke(ds, ClosedPoly(rc,
                new Vector2(400, 110), new Vector2(520, 290), new Vector2(400, 470), new Vector2(280, 290)), w);
            Line(ds, 400, 110, 400, 470, w * 0.6f);
            Line(ds, 280, 290, 520, 290, w * 0.6f);

            // string with bows
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 470);
                b.AddCubicBezier(new Vector2(300, 560), new Vector2(430, 590), new Vector2(500, 575));
            }), w * 0.7f);

            Vector2[] bows =
            {
                new(350, 528), new(430, 570), new(472, 572),
            };
            foreach (var b in bows)
            {
                Stroke(ds, ClosedPoly(rc,
                    b + new Vector2(-16, -10), b + new Vector2(16, 10),
                    b + new Vector2(-16, 10), b + new Vector2(-16, -10)), w * 0.6f);
                Stroke(ds, ClosedPoly(rc,
                    b + new Vector2(-16, -10), b + new Vector2(16, -10),
                    b + new Vector2(16, 10), b + new Vector2(-16, -10)), w * 0.6f);
            }

            // clouds
            Stroke(ds, Circle(rc, 170, 150, 34), w);
            Stroke(ds, Circle(rc, 220, 132, 42), w);
            Stroke(ds, Circle(rc, 268, 155, 30), w);
            Stroke(ds, Circle(rc, 590, 200, 30), w);
            Stroke(ds, Circle(rc, 634, 185, 38), w);
        }

        private static void DrawCupcake(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // frosting swirl
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(290, 355);
                b.AddQuadraticBezier(new Vector2(280, 285), new Vector2(340, 268));
                b.AddQuadraticBezier(new Vector2(350, 205), new Vector2(400, 205));
                b.AddQuadraticBezier(new Vector2(450, 205), new Vector2(460, 268));
                b.AddQuadraticBezier(new Vector2(520, 285), new Vector2(510, 355));
            }), w);

            // cherry
            Stroke(ds, Circle(rc, 400, 172, 24), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 148);
                b.AddQuadraticBezier(new Vector2(414, 116), new Vector2(442, 108));
            }), w * 0.7f);

            // wrapper
            Stroke(ds, ClosedPoly(rc,
                new Vector2(290, 365), new Vector2(510, 365), new Vector2(478, 510), new Vector2(322, 510)), w);
            Line(ds, 344, 365, 356, 505, w * 0.6f);
            Line(ds, 400, 365, 400, 505, w * 0.6f);
            Line(ds, 456, 365, 444, 505, w * 0.6f);

            // sprinkles
            Line(ds, 340, 300, 356, 312, w * 0.6f);
            Line(ds, 448, 296, 462, 310, w * 0.6f);
            Line(ds, 396, 238, 412, 246, w * 0.6f);

            // plate
            Stroke(ds, Ellipse(rc, 400, 528, 168, 22), w);
        }

        private static void DrawForest(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // sun
            Stroke(ds, Circle(rc, 105, 95, 38), w);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * (float)Math.PI / 180f;
                var dir = new Vector2((float)Math.Sin(a), -(float)Math.Cos(a));
                var p1 = new Vector2(105, 95) + dir * 50;
                var p2 = new Vector2(105, 95) + dir * 70;
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w * 0.7f);
            }

            // hills
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(40, 520);
                b.AddQuadraticBezier(new Vector2(300, 380), new Vector2(560, 520));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(440, 520);
                b.AddQuadraticBezier(new Vector2(640, 420), new Vector2(790, 520));
            }), w);

            void Pine(float cx, float top, float width, float trunkH)
            {
                Stroke(ds, ClosedPoly(rc,
                    new Vector2(cx, top),
                    new Vector2(cx - width, top + width * 1.15f),
                    new Vector2(cx + width, top + width * 1.15f)), w);
                Stroke(ds, ClosedPoly(rc,
                    new Vector2(cx, top + width * 0.75f),
                    new Vector2(cx - width * 1.22f, top + width * 2.1f),
                    new Vector2(cx + width * 1.22f, top + width * 2.1f)), w);
                Line(ds, cx, top + width * 2.1f, cx, top + width * 2.1f + trunkH, w);
            }

            Pine(210, 200, 78, 34);
            Pine(430, 240, 62, 28);
            Pine(630, 260, 52, 24);

            // birds
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(480, 140);
                b.AddQuadraticBezier(new Vector2(500, 124), new Vector2(520, 140));
            }), w * 0.6f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(545, 165);
                b.AddQuadraticBezier(new Vector2(562, 152), new Vector2(579, 165));
            }), w * 0.6f);
        }

        private static void DrawUndersea(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;

            // sand bed
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(30, 500);
                b.AddQuadraticBezier(new Vector2(250, 462), new Vector2(430, 498));
                b.AddQuadraticBezier(new Vector2(620, 534), new Vector2(780, 486));
            }), w);
            Dot(ds, 180, 528, 6);
            Dot(ds, 300, 540, 7);
            Dot(ds, 560, 545, 6);
            Dot(ds, 660, 522, 7);

            // seaweed
            for (int i = 0; i < 3; i++)
            {
                float bx = 105 + i * 46;
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(bx, 505);
                    b.AddCubicBezier(
                        new Vector2(bx + 26, 430), new Vector2(bx - 26, 370),
                        new Vector2(bx + 12, 285 - i * 18));
                }), w * 0.8f);
            }

            // big fish
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(300, 300);
                b.AddQuadraticBezier(new Vector2(420, 200), new Vector2(540, 300));
                b.AddQuadraticBezier(new Vector2(420, 400), new Vector2(300, 300));
            }, close: true), w);
            Stroke(ds, ClosedPoly(rc,
                new Vector2(538, 300), new Vector2(625, 240), new Vector2(600, 300), new Vector2(625, 360)), w);
            Dot(ds, 350, 275, 10);
            Stroke(ds, ClosedPoly(rc, new Vector2(420, 220), new Vector2(452, 160), new Vector2(482, 232)), w);

            // small fish
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(430, 430);
                b.AddQuadraticBezier(new Vector2(500, 385), new Vector2(570, 430));
                b.AddQuadraticBezier(new Vector2(500, 475), new Vector2(430, 430));
            }, close: true), w);
            Stroke(ds, ClosedPoly(rc,
                new Vector2(568, 430), new Vector2(620, 398), new Vector2(620, 462)), w);
            Dot(ds, 458, 418, 6);

            // starfish
            var star = new List<Vector2>();
            var starCenter = new Vector2(210, 520);
            for (int i = 0; i < 10; i++)
            {
                float r = (i % 2 == 0) ? 46 : 20;
                float a = i * 36f * (float)Math.PI / 180f - (float)Math.PI / 2;
                star.Add(starCenter + new Vector2(r * (float)Math.Cos(a), r * (float)Math.Sin(a)));
            }

            Stroke(ds, ClosedPoly(rc, star.ToArray()), w);
            Dot(ds, starCenter.X, starCenter.Y - 6, 5);

            // bubbles
            Stroke(ds, Circle(rc, 690, 260, 20), w);
            Stroke(ds, Circle(rc, 715, 200, 14), w);
            Stroke(ds, Circle(rc, 678, 150, 10), w);
        }

        private static void DrawRose(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(400, 205);
            // outer petals
            for (int i = 0; i < 8; i++)
            {
                float ang = i * 45f;
                var pc = c + Vector2.Transform(new Vector2(0, -78), Matrix3x2.CreateRotation(ang * (float)Math.PI / 180f));
                var petal = Rotated(Ellipse(rc, 400, 127, 66, 32), ang, pc);
                Stroke(ds, petal, w);
                // petal vein
                var dir = Vector2.Normalize(pc - c);
                Line(ds, c.X + dir.X * 30, c.Y + dir.Y * 30, pc.X, pc.Y, w * 0.45f);
            }
            // middle + inner swirl
            Stroke(ds, Circle(rc, c.X, c.Y, 52), w);
            Stroke(ds, Circle(rc, c.X, c.Y, 32), w * 0.8f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(c.X - 22, c.Y + 6);
                b.AddQuadraticBezier(c + new Vector2(-24, -22), c + new Vector2(2, -20));
                b.AddQuadraticBezier(c + new Vector2(26, -18), c + new Vector2(20, 8));
                b.AddQuadraticBezier(c + new Vector2(14, 26), c + new Vector2(-12, 22));
            }), w * 0.7f);
            Dot(ds, c.X, c.Y, 6);
            // sepals under bloom
            for (int i = -2; i <= 2; i++)
                Stroke(ds, Leaf(rc, c + new Vector2(i * 22, 48), c + new Vector2(i * 34, 92), 12), w * 0.7f);
            // main stem with thorns
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 295);
                b.AddCubicBezier(new Vector2(390, 380), new Vector2(412, 460), new Vector2(400, 548));
            }), w);
            for (int i = 0; i < 3; i++)
            {
                float y = 370 + i * 45;
                Stroke(ds, ClosedPoly(rc, new Vector2(398, y), new Vector2(378, y + 10), new Vector2(398, y + 18)), w * 0.6f);
                Stroke(ds, ClosedPoly(rc, new Vector2(402, y + 22), new Vector2(422, y + 30), new Vector2(402, y + 38)), w * 0.6f);
            }
            // leaves with veins
            var leaves = new (Vector2 b, Vector2 t)[] { (new(396, 420), new(290, 390)), (new(404, 460), new(510, 430)), (new(398, 500), new(300, 490)) };
            foreach (var (b, t) in leaves)
            {
                Stroke(ds, Leaf(rc, b, t, 30), w);
                Line(ds, b.X, b.Y, t.X, t.Y, w * 0.5f);
                for (int k = 1; k <= 3; k++)
                {
                    var p = b + (t - b) * (k / 4f);
                    Line(ds, p.X, p.Y, p.X - 22, p.Y - 14, w * 0.4f);
                    Line(ds, p.X, p.Y, p.X + 22, p.Y - 10, w * 0.4f);
                }
            }
            // side buds
            foreach (float bx in new float[] { 200, 600 })
            {
                float by = 330;
                Stroke(ds, Leaf(rc, new Vector2(bx, by + 38), new Vector2(bx, by - 30), 26), w);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(bx - 18, by + 10);
                    b.AddQuadraticBezier(new Vector2(bx, by - 8), new Vector2(bx + 18, by + 10));
                }), w * 0.6f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(bx, by + 38);
                    b.AddQuadraticBezier(new Vector2(bx + (bx < 400 ? 40 : -40), 440), new Vector2(bx + (bx < 400 ? 60 : -60), 545));
                }), w * 0.8f);
            }
            // grass + tiny daisies + bee
            Line(ds, 110, 548, 690, 548, w);
            for (int i = 0; i < 9; i++)
            {
                float gx = 140 + i * 62;
                Stroke(ds, Poly(rc, new Vector2(gx, 548), new Vector2(gx + 7, 524 - (i % 3) * 8)), w * 0.55f);
            }
            foreach (var d in new Vector2[] { new(150, 480), new(650, 470) })
            {
                for (int p = 0; p < 6; p++)
                {
                    float a = p * 60f * (float)Math.PI / 180f;
                    Stroke(ds, Ellipse(rc, d.X + 16 * (float)Math.Cos(a), d.Y + 16 * (float)Math.Sin(a), 12, 8), w * 0.55f);
                }
                Stroke(ds, Circle(rc, d.X, d.Y, 9), w * 0.6f);
                Line(ds, d.X, d.Y + 20, d.X, d.Y + 58, w * 0.55f);
            }
            // bee
            Stroke(ds, Ellipse(rc, 560, 140, 26, 18), w * 0.7f);
            Stroke(ds, Ellipse(rc, 548, 118, 14, 10), w * 0.55f);
            Stroke(ds, Ellipse(rc, 572, 118, 14, 10), w * 0.55f);
            Line(ds, 552, 128, 552, 152, w * 0.5f);
            Line(ds, 566, 128, 566, 152, w * 0.5f);
        }

        private static void DrawMushrooms(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            void Mushroom(float cx, float baseY, float capW, float capH, float stemW, float stemH)
            {
                float capY = baseY - stemH - capH * 0.6f;
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(cx - capW, capY + capH * 0.7f);
                    b.AddQuadraticBezier(new Vector2(cx - capW * 0.7f, capY - capH * 0.7f), new Vector2(cx, capY - capH * 0.7f));
                    b.AddQuadraticBezier(new Vector2(cx + capW * 0.7f, capY - capH * 0.7f), new Vector2(cx + capW, capY + capH * 0.7f));
                    b.AddQuadraticBezier(new Vector2(cx, capY + capH * 0.25f), new Vector2(cx - capW, capY + capH * 0.7f));
                }, close: true), w);
                Line(ds, cx - capW + 8, capY + capH * 0.62f, cx + capW - 8, capY + capH * 0.62f, w * 0.6f);
                for (int i = -2; i <= 2; i++)
                    Line(ds, cx + i * (stemW * 0.32f), capY + capH * 0.62f, cx + i * (stemW * 0.28f), baseY - 4, w * 0.4f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(cx - stemW / 2, capY + capH * 0.55f);
                    b.AddLine(new Vector2(cx - stemW / 2 - 4, baseY));
                    b.AddLine(new Vector2(cx + stemW / 2 + 4, baseY));
                    b.AddLine(new Vector2(cx + stemW / 2, capY + capH * 0.55f));
                }), w);
            }
            Mushroom(280, 500, 130, 90, 64, 170);
            Mushroom(520, 510, 150, 100, 76, 200);
            Mushroom(660, 505, 80, 60, 44, 120);
            Mushroom(150, 505, 66, 52, 38, 100);
            // cap spots
            foreach (var s in new (float x, float y, float r)[] { (240, 250, 18), (300, 235, 24), (330, 275, 12), (480, 240, 20), (550, 225, 26), (590, 270, 13), (650, 350, 10), (140, 385, 9) })
                Stroke(ds, Circle(rc, s.x, s.y, s.r), w * 0.6f);
            // dots texture on stems
            foreach (var d in new Vector2[] { new(270, 420), new(295, 450), new(510, 430), new(535, 465), new(655, 450) })
                Dot(ds, d.X, d.Y, 4);
            // fern fronds
            for (int f = 0; f < 2; f++)
            {
                float bx = f == 0 ? 90 : 730;
                float dir = f == 0 ? 1 : -1;
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(bx, 510);
                    b.AddQuadraticBezier(new Vector2(bx + dir * 40, 420), new Vector2(bx + dir * 30, 320));
                }), w * 0.8f);
                for (int i = 0; i < 7; i++)
                {
                    float t = i / 7f;
                    float fy = 490 - t * 150;
                    float fx = bx + dir * (40 * (float)Math.Sin(t * 2.2f) + t * 30);
                    Stroke(ds, Leaf(rc, new Vector2(fx, fy), new Vector2(fx + dir * 44, fy - 22), 10), w * 0.55f);
                    Stroke(ds, Leaf(rc, new Vector2(fx, fy), new Vector2(fx - dir * 8, fy - 40), 9), w * 0.55f);
                }
            }
            // grass + pebbles + ground
            Line(ds, 60, 512, 750, 512, w);
            var rnd = new Random(7);
            for (int i = 0; i < 16; i++)
            {
                float gx = 70 + i * 44;
                Stroke(ds, Poly(rc, new Vector2(gx, 512), new Vector2(gx + 6, 488 - rnd.Next(0, 18))), w * 0.5f);
            }
            foreach (var p in new (float x, float y, float rx, float ry)[] { (380, 528, 30, 10), (450, 532, 22, 8), (200, 530, 18, 7) })
                Stroke(ds, Ellipse(rc, p.x, p.y, p.rx, p.ry), w * 0.6f);
        }

        private static void DrawLotus(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            void Lotus(Vector2 c, float s)
            {
                for (int i = -3; i <= 3; i++)
                {
                    float ang = i * 22f;
                    var tip = c + Vector2.Transform(new Vector2(0, -95 * s), Matrix3x2.CreateRotation(ang * (float)Math.PI / 180f));
                    Stroke(ds, Leaf(rc, c + new Vector2(0, 10 * s), tip, 30 * s), w * 0.9f);
                }
                Stroke(ds, Ellipse(rc, c.X, c.Y + 18 * s, 30 * s, 14 * s), w * 0.7f);
                for (int i = -2; i <= 2; i++)
                    Dot(ds, c.X + i * 10 * s, c.Y + 16 * s, 3.5f);
            }
            Lotus(new Vector2(400, 300), 1.25f);
            Lotus(new Vector2(185, 360), 0.8f);
            Lotus(new Vector2(615, 360), 0.8f);
            // lily pads with veins
            foreach (var pad in new (float x, float y, float rx, float ry)[] { (320, 470, 90, 26), (500, 480, 100, 28), (140, 470, 60, 18), (670, 475, 62, 18) })
            {
                Stroke(ds, Ellipse(rc, pad.x, pad.y, pad.rx, pad.ry), w);
                Line(ds, pad.x, pad.y, pad.x + pad.rx * 0.35f, pad.y - pad.ry * 0.3f, w * 0.7f);
                Line(ds, pad.x, pad.y, pad.x + pad.rx * 0.35f, pad.y + pad.ry * 0.3f, w * 0.7f);
                for (int i = -3; i <= 3; i++)
                    Line(ds, pad.x, pad.y, pad.x + i * pad.rx * 0.26f, pad.y + (i == 0 ? -pad.ry : pad.ry * 0.85f), w * 0.45f);
            }
            // water ripples
            for (int i = 0; i < 3; i++)
                Stroke(ds, Ellipse(rc, 400, 540 - i * 12, 220 - i * 30, 10), w * 0.5f);
            // reeds + cattails
            foreach (float rx in new float[] { 70, 95, 715, 740 })
            {
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(rx, 500);
                    b.AddQuadraticBezier(new Vector2(rx + 10, 380), new Vector2(rx - 6, 250));
                }), w * 0.8f);
                Stroke(ds, RoundedRect(rc, rx - 22, 190, 32, 66, 14), w * 0.8f);
                Line(ds, rx - 6, 256, rx - 6, 230, w * 0.7f);
            }
            // dragonfly
            Stroke(ds, Ellipse(rc, 560, 150, 34, 10), w * 0.6f);
            Stroke(ds, Ellipse(rc, 560, 172, 30, 9), w * 0.6f);
            Stroke(ds, Ellipse(rc, 496, 150, 34, 10), w * 0.6f);
            Stroke(ds, Ellipse(rc, 496, 172, 30, 9), w * 0.6f);
            Stroke(ds, RoundedRect(rc, 520, 120, 16, 70, 8), w * 0.7f);
            Dot(ds, 528, 116, 7);
            // sun + clouds
            Stroke(ds, Circle(rc, 400, 80, 34), w * 0.8f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * (float)Math.PI / 180f;
                Line(ds, 400 + 46 * (float)Math.Sin(a), 80 - 46 * (float)Math.Cos(a), 400 + 64 * (float)Math.Sin(a), 80 - 64 * (float)Math.Cos(a), w * 0.6f);
            }
        }

        private static void DrawOak(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // roots + trunk with bark
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(340, 520);
                b.AddCubicBezier(new Vector2(355, 440), new Vector2(350, 380), new Vector2(360, 320));
                b.AddLine(new Vector2(440, 320));
                b.AddCubicBezier(new Vector2(450, 390), new Vector2(445, 450), new Vector2(462, 520));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(340, 520);
                b.AddQuadraticBezier(new Vector2(300, 528), new Vector2(272, 548));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(462, 520);
                b.AddQuadraticBezier(new Vector2(502, 528), new Vector2(530, 548));
            }), w);
            for (int i = 0; i < 4; i++)
            {
                float x = 372 + i * 20;
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(x, 510);
                    b.AddQuadraticBezier(new Vector2(x + 6, 430), new Vector2(x - 4, 335));
                }), w * 0.45f);
            }
            // branches
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(368, 380);
                b.AddQuadraticBezier(new Vector2(280, 340), new Vector2(230, 260));
            }), w * 0.9f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(432, 380);
                b.AddQuadraticBezier(new Vector2(520, 340), new Vector2(570, 260));
            }), w * 0.9f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 350);
                b.AddLine(new Vector2(400, 250));
            }), w * 0.9f);
            // foliage clusters
            var blobs = new (float x, float y, float r)[] { (400, 170, 110), (270, 210, 80), (530, 210, 80), (200, 280, 60), (600, 280, 60), (330, 130, 66), (470, 130, 66) };
            foreach (var b2 in blobs)
                Stroke(ds, Circle(rc, b2.x, b2.y, b2.r), w);
            // leaf texture arcs
            var rnd = new Random(11);
            for (int i = 0; i < 26; i++)
            {
                float x = 220 + rnd.Next(0, 360);
                float y = 90 + rnd.Next(0, 170);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(x, y);
                    b.AddQuadraticBezier(new Vector2(x + 12, y - 10), new Vector2(x + 24, y));
                }), w * 0.4f);
            }
            // acorns
            foreach (var a in new Vector2[] { new(300, 300), new(500, 300), new(400, 260), new(240, 330), new(560, 330), new(360, 320) })
            {
                Stroke(ds, Ellipse(rc, a.X, a.Y + 14, 14, 18), w * 0.65f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(a.X - 16, a.Y + 2);
                    b.AddQuadraticBezier(new Vector2(a.X, a.Y - 12), new Vector2(a.X + 16, a.Y + 2));
                    b.AddQuadraticBezier(new Vector2(a.X, a.Y + 10), new Vector2(a.X - 16, a.Y + 2));
                }, close: true), w * 0.65f);
                Line(ds, a.X, a.Y - 10, a.X + 6, a.Y - 22, w * 0.55f);
            }
            // tire swing
            Line(ds, 500, 352, 470, 470, w * 0.7f);
            Line(ds, 540, 352, 570, 470, w * 0.7f);
            Stroke(ds, Ellipse(rc, 520, 492, 52, 30), w);
            Stroke(ds, Ellipse(rc, 520, 492, 30, 16), w * 0.7f);
            Line(ds, 150, 552, 660, 552, w);
        }

        private static void DrawOwl(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // moon + stars
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(660, 90);
                b.AddQuadraticBezier(new Vector2(610, 130), new Vector2(660, 175));
                b.AddQuadraticBezier(new Vector2(625, 130), new Vector2(660, 90));
            }, close: true), w * 0.8f);
            var rnd = new Random(21);
            for (int i = 0; i < 18; i++)
            {
                float x = 60 + rnd.Next(0, 500);
                float y = 50 + rnd.Next(0, 160);
                if (i % 3 == 0) { Line(ds, x - 8, y, x + 8, y, w * 0.45f); Line(ds, x, y - 8, x, y + 8, w * 0.45f); }
                else Dot(ds, x, y, 4);
            }
            // branch
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(90, 440);
                b.AddCubicBezier(new Vector2(280, 420), new Vector2(520, 460), new Vector2(720, 430));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(620, 438);
                b.AddQuadraticBezier(new Vector2(660, 400), new Vector2(700, 395));
            }), w * 0.7f);
            foreach (var lf in new (Vector2 b, Vector2 t)[] { (new(250, 432), new(200, 370)), (new(350, 438), new(360, 372)), (new(560, 448), new(600, 392)) })
            {
                Stroke(ds, Leaf(rc, lf.b, lf.t, 22), w * 0.75f);
                Line(ds, lf.b.X, lf.b.Y, lf.t.X, lf.t.Y, w * 0.4f);
            }
            // body
            Stroke(ds, Ellipse(rc, 400, 320, 120, 140), w);
            // ear tufts
            Stroke(ds, ClosedPoly(rc, new Vector2(300, 210), new Vector2(275, 130), new Vector2(345, 180)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(500, 210), new Vector2(525, 130), new Vector2(455, 180)), w);
            // facial disc
            Stroke(ds, Circle(rc, 355, 260, 52), w * 0.8f);
            Stroke(ds, Circle(rc, 445, 260, 52), w * 0.8f);
            // eyes
            foreach (float ex in new float[] { 355, 445 })
            {
                Stroke(ds, Circle(rc, ex, 260, 30), w * 0.8f);
                Stroke(ds, Circle(rc, ex, 260, 14), w * 0.6f);
                Dot(ds, ex, 260, 7);
            }
            // beak
            Stroke(ds, ClosedPoly(rc, new Vector2(388, 290), new Vector2(412, 290), new Vector2(400, 310)), w * 0.8f);
            // belly scallops 3 rows
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 5; col++)
                {
                    float x = 330 + col * 35 + (row % 2) * 12;
                    float y = 340 + row * 32;
                    Stroke(ds, Path(rc, b =>
                    {
                        b.BeginFigure(x, y);
                        b.AddQuadraticBezier(new Vector2(x + 12, y + 16), new Vector2(x + 26, y));
                    }), w * 0.5f);
                }
            // wings with feather lines
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(288, 300);
                b.AddQuadraticBezier(new Vector2(260, 380), new Vector2(300, 430));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(512, 300);
                b.AddQuadraticBezier(new Vector2(540, 380), new Vector2(500, 430));
            }), w);
            for (int i = 0; i < 3; i++)
            {
                Line(ds, 282, 330 + i * 28, 306, 336 + i * 28, w * 0.45f);
                Line(ds, 518, 330 + i * 28, 494, 336 + i * 28, w * 0.45f);
            }
            // feet
            foreach (float fx in new float[] { 350, 450 })
            {
                for (int t = -1; t <= 1; t++)
                {
                    Line(ds, fx, 452, fx + t * 18, 472, w * 0.8f);
                    Stroke(ds, Poly(rc, new Vector2(fx + t * 18, 472), new Vector2(fx + t * 18 + 4, 480)), w * 0.6f);
                }
            }
        }

        private static void DrawPeacock(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // tail fan eyes
            for (int i = 0; i < 11; i++)
            {
                float ang = -75 + i * 15;
                float rad = ang * (float)Math.PI / 180f;
                var dir = new Vector2((float)Math.Sin(rad), -(float)Math.Cos(rad));
                var baseP = new Vector2(400, 400);
                var tip = baseP + dir * 250;
                Line(ds, baseP.X, baseP.Y, tip.X, tip.Y, w * 0.55f);
                var outer = Ellipse(rc, tip.X, tip.Y, 30, 40);
                Stroke(ds, Rotated(outer, ang, tip), w * 0.85f);
                var inner = Ellipse(rc, tip.X, tip.Y, 15, 20);
                Stroke(ds, Rotated(inner, ang, tip), w * 0.6f);
                Dot(ds, tip.X, tip.Y, 7);
                // side barbs
                var mid = baseP + dir * 140;
                var perp = new Vector2(-dir.Y, dir.X);
                Line(ds, mid.X, mid.Y, mid.X + perp.X * 22, mid.Y + perp.Y * 22, w * 0.4f);
                Line(ds, mid.X, mid.Y, mid.X - perp.X * 22, mid.Y - perp.Y * 22, w * 0.4f);
            }
            // body
            Stroke(ds, Ellipse(rc, 400, 430, 62, 80), w);
            // breast scales
            for (int r = 0; r < 4; r++)
                for (int c2 = -2; c2 <= 2; c2++)
                {
                    float x = 400 + c2 * 22;
                    float y = 390 + r * 26;
                    Stroke(ds, Path(rc, b =>
                    {
                        b.BeginFigure(x - 10, y);
                        b.AddQuadraticBezier(new Vector2(x, y + 14), new Vector2(x + 10, y));
                    }), w * 0.45f);
                }
            // neck + head
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(380, 370);
                b.AddCubicBezier(new Vector2(375, 300), new Vector2(385, 260), new Vector2(400, 220));
                b.AddCubicBezier(new Vector2(410, 260), new Vector2(420, 300), new Vector2(420, 370));
            }), w);
            Stroke(ds, Circle(rc, 400, 195, 28), w);
            Dot(ds, 390, 190, 5);
            Stroke(ds, ClosedPoly(rc, new Vector2(400, 195), new Vector2(428, 202), new Vector2(400, 210)), w * 0.7f);
            // crest
            for (int i = -1; i <= 1; i++)
            {
                Line(ds, 400, 168, 400 + i * 22, 132, w * 0.6f);
                Stroke(ds, Circle(rc, 400 + i * 22, 128, 8), w * 0.6f);
            }
            // feet + ground
            Line(ds, 380, 508, 380, 540, w);
            Line(ds, 420, 508, 420, 540, w);
            foreach (float fx in new float[] { 380, 420 })
                for (int t = -1; t <= 1; t++)
                    Line(ds, fx, 540, fx + t * 14, 552, w * 0.6f);
            Line(ds, 120, 554, 680, 554, w * 0.8f);
        }

        private static void DrawSeaTurtle(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // shell
            Stroke(ds, Ellipse(rc, 400, 320, 170, 125), w);
            Stroke(ds, Ellipse(rc, 400, 320, 190, 140), w * 0.8f);
            // scutes: central row + sides
            for (int i = -2; i <= 2; i++)
                Stroke(ds, ClosedPoly(rc, new Vector2(400 + i * 52 - 26, 250), new Vector2(400 + i * 52 + 26, 250), new Vector2(400 + i * 52 + 30, 320), new Vector2(400 + i * 52 - 30, 320)), w * 0.6f);
            for (int i = -1; i <= 1; i++)
                Stroke(ds, Ellipse(rc, 400 + i * 70, 370, 32, 24), w * 0.55f);
            // rim ticks
            for (int i = 0; i < 16; i++)
            {
                float a = i * 22.5f * (float)Math.PI / 180f;
                var p1 = new Vector2(400, 320) + new Vector2(170 * (float)Math.Cos(a), 125 * (float)Math.Sin(a));
                var p2 = new Vector2(400, 320) + new Vector2(190 * (float)Math.Cos(a), 140 * (float)Math.Sin(a));
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w * 0.45f);
            }
            // head
            Stroke(ds, Circle(rc, 400, 140, 48), w);
            Dot(ds, 382, 132, 7);
            Dot(ds, 418, 132, 7);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(384, 162);
                b.AddQuadraticBezier(new Vector2(400, 172), new Vector2(416, 162));
            }), w * 0.6f);
            Line(ds, 400, 188, 400, 205, w * 0.7f);
            // flippers with scales
            var flippers = new Vector2[][]
            {
                new Vector2[]{ new Vector2(250,300), new Vector2(150,260), new Vector2(130,320), new Vector2(235,350)},
                new Vector2[]{ new Vector2(550,300), new Vector2(650,260), new Vector2(670,320), new Vector2(565,350)},
                new Vector2[]{ new Vector2(300,420), new Vector2(240,500), new Vector2(300,510), new Vector2(350,430)},
                new Vector2[]{ new Vector2(500,420), new Vector2(560,500), new Vector2(500,510), new Vector2(450,430)},
            };
            foreach (var f in flippers)
            {
                Stroke(ds, ClosedPoly(rc, f), w);
                for (int k = 0; k < 3; k++)
                    Line(ds, f[0].X + k * 18, f[0].Y + 10 + k * 8, f[3].X + k * 10, f[3].Y - 6, w * 0.4f);
            }
            // coral + seaweed + bubbles + sand
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(110, 510);
                b.AddQuadraticBezier(new Vector2(120, 440), new Vector2(90, 410));
                b.AddQuadraticBezier(new Vector2(130, 420), new Vector2(140, 380));
            }), w * 0.7f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(690, 515);
                b.AddCubicBezier(new Vector2(700, 450), new Vector2(670, 430), new Vector2(690, 370));
            }), w * 0.7f);
            Stroke(ds, Circle(rc, 150, 180, 16), w * 0.6f);
            Stroke(ds, Circle(rc, 180, 130, 11), w * 0.6f);
            Stroke(ds, Circle(rc, 650, 150, 18), w * 0.6f);
            Stroke(ds, Circle(rc, 680, 100, 12), w * 0.6f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(80, 540);
                b.AddQuadraticBezier(new Vector2(400, 510), new Vector2(720, 540));
            }), w);
        }

        private static void DrawElephant(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // body + head
            Stroke(ds, RoundedRect(rc, 250, 280, 300, 190, 90), w);
            Stroke(ds, Circle(rc, 580, 300, 90), w);
            // ear with pattern
            Stroke(ds, Ellipse(rc, 540, 300, 55, 70), w * 0.8f);
            Stroke(ds, Ellipse(rc, 540, 300, 34, 46), w * 0.55f);
            for (int i = 0; i < 6; i++)
                Dot(ds, 540 + 22 * (float)Math.Cos(i * 60 * Math.PI / 180), 300 + 30 * (float)Math.Sin(i * 60 * Math.PI / 180), 3.5f);
            // trunk
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(640, 330);
                b.AddCubicBezier(new Vector2(690, 380), new Vector2(660, 460), new Vector2(620, 490));
                b.AddCubicBezier(new Vector2(600, 450), new Vector2(620, 390), new Vector2(600, 340));
            }), w);
            for (int i = 0; i < 3; i++)
                Stroke(ds, Path(rc, b =>
                {
                    float y = 400 + i * 28;
                    b.BeginFigure(668 - i * 6, y);
                    b.AddQuadraticBezier(new Vector2(648 - i * 6, y + 10), new Vector2(626 - i * 4, y + 6));
                }), w * 0.5f);
            Stroke(ds, Ellipse(rc, 620, 492, 16, 8), w * 0.6f);
            // tusk
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(600, 360);
                b.AddQuadraticBezier(new Vector2(640, 390), new Vector2(636, 430));
                b.AddQuadraticBezier(new Vector2(620, 400), new Vector2(590, 380));
            }, close: true), w * 0.8f);
            // eye + dots
            Dot(ds, 580, 285, 8);
            Stroke(ds, Circle(rc, 580, 285, 18), w * 0.6f);
            for (int i = 0; i < 3; i++)
                Line(ds, 592, 272 + i * 8, 606, 268 + i * 8, w * 0.45f);
            // blanket with diamonds + tassels
            Stroke(ds, RoundedRect(rc, 300, 250, 180, 120, 10), w);
            Stroke(ds, RoundedRect(rc, 312, 262, 156, 96, 8), w * 0.6f);
            for (int r = 0; r < 3; r++)
                for (int c2 = 0; c2 < 4; c2++)
                {
                    float x = 340 + c2 * 36;
                    float y = 286 + r * 28;
                    Stroke(ds, ClosedPoly(rc, new Vector2(x, y - 12), new Vector2(x + 12, y), new Vector2(x, y + 12), new Vector2(x - 12, y)), w * 0.45f);
                    Dot(ds, x, y, 3);
                }
            for (int i = 0; i < 5; i++)
            {
                float x = 312 + i * 39;
                Line(ds, x, 358, x, 378, w * 0.55f);
                Dot(ds, x, 384, 5);
            }
            // legs + toenails
            foreach (float lx in new float[] { 290, 380, 470 })
            {
                Line(ds, lx, 460, lx, 530, w);
                Line(ds, lx + 44, 460, lx + 44, 530, w);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(lx - 4, 530);
                    b.AddLine(new Vector2(lx + 48, 530));
                }), w);
                for (int n = 0; n < 3; n++)
                    Stroke(ds, Path(rc, b =>
                    {
                        b.BeginFigure(lx + 6 + n * 14, 530);
                        b.AddQuadraticBezier(new Vector2(lx + 10 + n * 14, 518), new Vector2(lx + 16 + n * 14, 530));
                    }), w * 0.55f);
            }
            // tail
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(252, 320);
                b.AddQuadraticBezier(new Vector2(210, 380), new Vector2(230, 450));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(222, 448);
                b.AddLine(new Vector2(238, 448));
                b.AddLine(new Vector2(230, 478));
                b.AddLine(new Vector2(222, 448));
            }, close: true), w * 0.7f);
            Line(ds, 120, 552, 700, 552, w * 0.8f);
        }

        private static void DrawFox(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // pines behind
            foreach (var p in new (float x, float top, float s)[] { (140, 180, 1f), (660, 170, 1.15f), (600, 260, 0.7f) })
            {
                Stroke(ds, ClosedPoly(rc, new Vector2(p.x, p.top), new Vector2(p.x - 55 * p.s, p.top + 90 * p.s), new Vector2(p.x + 55 * p.s, p.top + 90 * p.s)), w * 0.8f);
                Stroke(ds, ClosedPoly(rc, new Vector2(p.x, p.top + 60 * p.s), new Vector2(p.x - 68 * p.s, p.top + 160 * p.s), new Vector2(p.x + 68 * p.s, p.top + 160 * p.s)), w * 0.8f);
                Line(ds, p.x, p.top + 160 * p.s, p.x, p.top + 190 * p.s, w * 0.8f);
            }
            // tail big
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(480, 480);
                b.AddCubicBezier(new Vector2(600, 480), new Vector2(620, 340), new Vector2(520, 300));
                b.AddCubicBezier(new Vector2(580, 340), new Vector2(570, 440), new Vector2(460, 440));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(520, 300);
                b.AddQuadraticBezier(new Vector2(480, 320), new Vector2(470, 360));
            }), w * 0.6f);
            for (int i = 0; i < 4; i++)
                Line(ds, 560 - i * 18, 420 + i * 12, 590 - i * 14, 430 + i * 12, w * 0.45f);
            // body
            Stroke(ds, Ellipse(rc, 360, 430, 95, 90), w);
            // chest zigzag
            for (int i = 0; i < 3; i++)
                Stroke(ds, Poly(rc, new Vector2(320, 360 + i * 28), new Vector2(360, 380 + i * 28), new Vector2(400, 360 + i * 28)), w * 0.6f);
            // head
            Stroke(ds, ClosedPoly(rc, new Vector2(280, 250), new Vector2(360, 180), new Vector2(440, 250), new Vector2(400, 330), new Vector2(320, 330)), w);
            // ears
            Stroke(ds, ClosedPoly(rc, new Vector2(292, 240), new Vector2(270, 140), new Vector2(350, 200)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(428, 240), new Vector2(450, 140), new Vector2(370, 200)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(300, 222), new Vector2(290, 170), new Vector2(330, 205)), w * 0.6f);
            Stroke(ds, ClosedPoly(rc, new Vector2(420, 222), new Vector2(430, 170), new Vector2(390, 205)), w * 0.6f);
            // face mask
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(312, 268);
                b.AddQuadraticBezier(new Vector2(360, 248), new Vector2(408, 268));
                b.AddQuadraticBezier(new Vector2(390, 310), new Vector2(360, 318));
                b.AddQuadraticBezier(new Vector2(330, 310), new Vector2(312, 268));
            }, close: true), w * 0.7f);
            Dot(ds, 336, 278, 7);
            Dot(ds, 384, 278, 7);
            Stroke(ds, ClosedPoly(rc, new Vector2(348, 300), new Vector2(372, 300), new Vector2(360, 314)), w * 0.7f);
            Line(ds, 360, 314, 360, 326, w * 0.6f);
            for (int i = 0; i < 2; i++)
            {
                Line(ds, 310, 296 + i * 14, 250, 288 + i * 16, w * 0.5f);
                Line(ds, 410, 296 + i * 14, 470, 288 + i * 16, w * 0.5f);
            }
            // paws
            Stroke(ds, Ellipse(rc, 320, 512, 28, 16), w * 0.8f);
            Stroke(ds, Ellipse(rc, 400, 512, 28, 16), w * 0.8f);
            Line(ds, 120, 534, 680, 534, w * 0.8f);
            // moon + stars
            Stroke(ds, Circle(rc, 400, 90, 30), w * 0.7f);
            Dot(ds, 250, 80, 4); Dot(ds, 550, 70, 4); Dot(ds, 480, 120, 3.5f);
        }

        private static void DrawOctopus(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // head dome + face
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(280, 300);
                b.AddCubicBezier(new Vector2(280, 150), new Vector2(520, 150), new Vector2(520, 300));
                b.AddQuadraticBezier(new Vector2(400, 330), new Vector2(280, 300));
            }, close: true), w);
            // head deco swirls + dots
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(350, 220);
                b.AddQuadraticBezier(new Vector2(370, 200), new Vector2(390, 220));
                b.AddQuadraticBezier(new Vector2(370, 235), new Vector2(350, 220));
            }), w * 0.55f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(430, 210);
                b.AddQuadraticBezier(new Vector2(455, 195), new Vector2(470, 215));
            }), w * 0.55f);
            Dot(ds, 400, 180, 5); Dot(ds, 370, 260, 4); Dot(ds, 430, 260, 4);
            Stroke(ds, Circle(rc, 340, 250, 26), w * 0.8f);
            Stroke(ds, Circle(rc, 460, 250, 26), w * 0.8f);
            Dot(ds, 340, 250, 9); Dot(ds, 460, 250, 9);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(375, 290);
                b.AddQuadraticBezier(new Vector2(400, 305), new Vector2(425, 290));
            }), w * 0.7f);
            // 8 arms fanning down
            float[] angles = { -68, -48, -28, -12, 12, 28, 48, 68 };
            foreach (float ad in angles)
            {
                float rad = ad * (float)Math.PI / 180f;
                var start = new Vector2(400 + 110 * (float)Math.Sin(rad * 0.6f), 315);
                var mid = start + new Vector2(160 * (float)Math.Sin(rad), 110);
                var tip = start + new Vector2(210 * (float)Math.Sin(rad), 190 + Math.Abs(ad) * 0.3f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(start.X, start.Y);
                    b.AddCubicBezier(mid + new Vector2(-18, 0), mid + new Vector2(18, 40), tip);
                }), w * 0.9f);
                // suckers
                for (int k = 1; k <= 4; k++)
                {
                    float t = k / 5f;
                    float sx = start.X + (tip.X - start.X) * t + (k % 2 == 0 ? 10 : -10);
                    float sy = start.Y + (tip.Y - start.Y) * t;
                    Stroke(ds, Circle(rc, sx, sy, 9 - k), w * 0.5f);
                }
                Dot(ds, tip.X, tip.Y, 5);
            }
            // reef + bubbles
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(60, 530);
                b.AddQuadraticBezier(new Vector2(250, 500), new Vector2(400, 528));
                b.AddQuadraticBezier(new Vector2(560, 556), new Vector2(740, 524));
            }), w);
            foreach (var bpos in new Vector2[] { new(150, 150), new(180, 100), new(650, 120), new(680, 70), new(620, 180) })
                Stroke(ds, Circle(rc, bpos.X, bpos.Y, 10 + (bpos.X % 13)), w * 0.6f);
            foreach (var cw in new (float x, float h)[] { (90, 120), (710, 140) })
            {
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(cw.x, 525);
                    b.AddCubicBezier(new Vector2(cw.x + 20, 460), new Vector2(cw.x - 20, 430), new Vector2(cw.x + 8, cw.h + 320));
                }), w * 0.6f);
            }
        }

        private static void DrawLion(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(400, 300);
            // outer mane zigzag (16 points)
            var mane = new List<Vector2>();
            for (int i = 0; i < 32; i++)
            {
                float r = (i % 2 == 0) ? 188 : 152;
                float a = i * (360f / 32) * (float)Math.PI / 180f - (float)Math.PI / 2;
                mane.Add(c + new Vector2(r * (float)Math.Cos(a), r * (float)Math.Sin(a)));
            }
            Stroke(ds, ClosedPoly(rc, mane.ToArray()), w);
            // inner mane scallop
            var inner = new List<Vector2>();
            for (int i = 0; i < 24; i++)
            {
                float r = (i % 2 == 0) ? 138 : 122;
                float a = i * 15f * (float)Math.PI / 180f;
                inner.Add(c + new Vector2(r * (float)Math.Cos(a), r * (float)Math.Sin(a)));
            }
            Stroke(ds, ClosedPoly(rc, inner.ToArray()), w * 0.7f);
            // face
            Stroke(ds, Circle(rc, c.X, c.Y, 108), w);
            // ears
            Stroke(ds, Circle(rc, 310, 210, 28), w * 0.8f);
            Stroke(ds, Circle(rc, 490, 210, 28), w * 0.8f);
            Stroke(ds, Circle(rc, 310, 210, 12), w * 0.55f);
            Stroke(ds, Circle(rc, 490, 210, 12), w * 0.55f);
            // eyes + brows
            Dot(ds, 356, 275, 8); Dot(ds, 444, 275, 8);
            Stroke(ds, Circle(rc, 356, 275, 20), w * 0.6f);
            Stroke(ds, Circle(rc, 444, 275, 20), w * 0.6f);
            Stroke(ds, Poly(rc, new Vector2(330, 245), new Vector2(356, 238), new Vector2(382, 245)), w * 0.6f);
            Stroke(ds, Poly(rc, new Vector2(418, 245), new Vector2(444, 238), new Vector2(470, 245)), w * 0.6f);
            // muzzle
            Stroke(ds, Ellipse(rc, 372, 338, 36, 28), w * 0.8f);
            Stroke(ds, Ellipse(rc, 428, 338, 36, 28), w * 0.8f);
            Stroke(ds, ClosedPoly(rc, new Vector2(384, 312), new Vector2(416, 312), new Vector2(400, 330)), w * 0.8f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 330);
                b.AddQuadraticBezier(new Vector2(400, 348), new Vector2(382, 352));
                b.AddQuadraticBezier(new Vector2(400, 348), new Vector2(418, 352));
            }), w * 0.65f);
            for (int i = 0; i < 3; i++)
            {
                Line(ds, 330, 330 + i * 14, 272, 322 + i * 16, w * 0.5f);
                Line(ds, 470, 330 + i * 14, 528, 322 + i * 16, w * 0.5f);
            }
            foreach (float wx in new float[] { 344, 364, 436, 456 })
                Dot(ds, wx, 326, 3);
            // mane texture + paws + ground
            for (int i = 0; i < 16; i++)
            {
                float a = i * 22.5f * (float)Math.PI / 180f;
                Line(ds, c.X + 122 * (float)Math.Cos(a), c.Y + 122 * (float)Math.Sin(a), c.X + 150 * (float)Math.Cos(a), c.Y + 150 * (float)Math.Sin(a), w * 0.4f);
            }
            Stroke(ds, Ellipse(rc, 340, 508, 44, 22), w * 0.85f);
            Stroke(ds, Ellipse(rc, 460, 508, 44, 22), w * 0.85f);
            for (int i = -1; i <= 1; i++)
            {
                Line(ds, 340 + i * 22, 508, 340 + i * 22, 528, w * 0.5f);
                Line(ds, 460 + i * 22, 508, 460 + i * 22, 528, w * 0.5f);
            }
            Line(ds, 140, 534, 660, 534, w * 0.8f);
        }

        private static void DrawCastle(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            void Tower(float x, float y, float tw, float th)
            {
                Stroke(ds, RoundedRect(rc, x, y, tw, th, 4), w);
                // crenellations
                for (int i = 0; i < 4; i++)
                    Stroke(ds, RoundedRect(rc, x + i * (tw / 4), y - 22, tw / 4, 24, 2), w * 0.85f);
                // bricks
                for (int r = 0; r < 4; r++)
                {
                    float by = y + 50 + r * 32;
                    Line(ds, x + 8, by, x + tw - 8, by, w * 0.4f);
                    for (int c2 = 0; c2 < 3; c2++)
                        Line(ds, x + 18 + c2 * 30 + (r % 2) * 12, by, x + 18 + c2 * 30 + (r % 2) * 12, by + 32, w * 0.35f);
                }
                // arched window
                Stroke(ds, RoundedRect(rc, x + tw / 2 - 18, y + 40, 36, 56, 16), w * 0.8f);
                Line(ds, x + tw / 2, y + 40, x + tw / 2, y + 96, w * 0.5f);
            }
            Tower(150, 260, 110, 260);
            Tower(540, 260, 110, 260);
            Tower(310, 190, 180, 330);
            // center tall turret + roof + flag
            Stroke(ds, ClosedPoly(rc, new Vector2(300, 190), new Vector2(400, 90), new Vector2(500, 190)), w);
            for (int i = 0; i < 4; i++)
                Line(ds, 330 + i * 40, 160 - i * 2, 330 + i * 40, 188, w * 0.45f);
            Line(ds, 400, 90, 400, 50, w);
            Stroke(ds, ClosedPoly(rc, new Vector2(400, 50), new Vector2(452, 62), new Vector2(400, 74)), w);
            // side roofs
            Stroke(ds, ClosedPoly(rc, new Vector2(140, 260), new Vector2(205, 190), new Vector2(270, 260)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(530, 260), new Vector2(595, 190), new Vector2(660, 260)), w);
            // main gate + drawbridge
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(355, 520);
                b.AddLine(new Vector2(355, 440));
                b.AddQuadraticBezier(new Vector2(400, 400), new Vector2(445, 440));
                b.AddLine(new Vector2(445, 520));
            }), w);
            Line(ds, 400, 420, 400, 520, w * 0.6f);
            for (int i = 0; i < 3; i++)
                Line(ds, 362, 450 + i * 20, 438, 450 + i * 20, w * 0.45f);
            Dot(ds, 378, 482, 5); Dot(ds, 422, 482, 5);
            // moat waves + path stones
            for (int i = 0; i < 7; i++)
                Stroke(ds, Path(rc, b =>
                {
                    float x = 120 + i * 90;
                    b.BeginFigure(x, 544);
                    b.AddQuadraticBezier(new Vector2(x + 20, 534), new Vector2(x + 40, 544));
                }), w * 0.6f);
            foreach (var s in new Vector2[] { new(370, 556), new(410, 566), new(390, 578) })
                Stroke(ds, Ellipse(rc, s.X, s.Y, 22, 8), w * 0.5f);
            // clouds + sun + birds
            Stroke(ds, Circle(rc, 680, 100, 34), w * 0.7f);
            foreach (var cl in new Vector2[] { new(140, 110), new(250, 90) })
            {
                Stroke(ds, Circle(rc, cl.X, cl.Y, 26), w * 0.6f);
                Stroke(ds, Circle(rc, cl.X + 34, cl.Y - 8, 32), w * 0.6f);
                Stroke(ds, Circle(rc, cl.X + 68, cl.Y, 24), w * 0.6f);
            }
        }

        private static void DrawDragon(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // body
            Stroke(ds, Ellipse(rc, 380, 420, 130, 100), w);
            // belly scales
            for (int r = 0; r < 4; r++)
                for (int c2 = 0; c2 < 4; c2++)
                {
                    float x = 330 + c2 * 34 + (r % 2) * 14;
                    float y = 380 + r * 30;
                    Stroke(ds, Path(rc, b =>
                    {
                        b.BeginFigure(x - 14, y);
                        b.AddQuadraticBezier(new Vector2(x, y + 14), new Vector2(x + 14, y));
                    }), w * 0.45f);
                }
            // head
            Stroke(ds, Circle(rc, 540, 240, 78), w);
            Stroke(ds, Ellipse(rc, 590, 270, 42, 30), w * 0.85f);
            Dot(ds, 596, 266, 5);
            Dot(ds, 540, 228, 11);
            Stroke(ds, Circle(rc, 540, 228, 22), w * 0.6f);
            // nostril + smile + horn + ear
            Dot(ds, 614, 268, 4);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(574, 292);
                b.AddQuadraticBezier(new Vector2(596, 300), new Vector2(614, 290));
            }), w * 0.6f);
            Stroke(ds, ClosedPoly(rc, new Vector2(500, 175), new Vector2(480, 110), new Vector2(530, 160)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(600, 190), new Vector2(640, 150), new Vector2(620, 200)), w * 0.8f);
            // back spikes
            for (int i = 0; i < 5; i++)
            {
                float x = 460 - i * 48;
                float y = 335 - i * 8;
                Stroke(ds, ClosedPoly(rc, new Vector2(x - 16, y), new Vector2(x, y - 34), new Vector2(x + 16, y)), w * 0.85f);
            }
            // wing
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 340);
                b.AddQuadraticBezier(new Vector2(340, 220), new Vector2(240, 200));
                b.AddQuadraticBezier(new Vector2(280, 250), new Vector2(270, 300));
                b.AddQuadraticBezier(new Vector2(310, 290), new Vector2(310, 340));
                b.AddQuadraticBezier(new Vector2(350, 330), new Vector2(400, 340));
            }, close: true), w);
            Line(ds, 400, 340, 240, 200, w * 0.55f);
            Line(ds, 360, 338, 270, 300, w * 0.5f);
            // arms + legs + claws
            Stroke(ds, RoundedRect(rc, 300, 440, 70, 34, 14), w * 0.85f);
            Stroke(ds, RoundedRect(rc, 430, 490, 80, 36, 14), w * 0.85f);
            for (int i = 0; i < 3; i++)
            {
                Line(ds, 306 + i * 18, 474, 300 + i * 18, 490, w * 0.6f);
                Line(ds, 436 + i * 20, 526, 430 + i * 20, 542, w * 0.6f);
            }
            // tail curl
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(252, 440);
                b.AddCubicBezier(new Vector2(170, 450), new Vector2(150, 540), new Vector2(230, 545));
            }), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(230, 545), new Vector2(262, 530), new Vector2(258, 562)), w * 0.8f);
            // fire puff + clouds
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(632, 270);
                b.AddQuadraticBezier(new Vector2(670, 250), new Vector2(690, 275));
                b.AddQuadraticBezier(new Vector2(710, 290), new Vector2(688, 310));
                b.AddQuadraticBezier(new Vector2(668, 328), new Vector2(640, 305));
            }), w * 0.7f);
            Dot(ds, 668, 288, 4);
            Stroke(ds, Circle(rc, 180, 140, 28), w * 0.6f);
            Stroke(ds, Circle(rc, 220, 130, 34), w * 0.6f);
            Line(ds, 120, 540, 680, 540, w * 0.7f);
        }

        private static void DrawUnicorn(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // rainbow behind
            for (int i = 0; i < 3; i++)
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(120 + i * 22, 480);
                    b.AddQuadraticBezier(new Vector2(400, 120 - i * 10), new Vector2(680 - i * 22, 480));
                }), w * (0.6f - i * 0.08f));
            // neck + head profile
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(330, 520);
                b.AddCubicBezier(new Vector2(340, 400), new Vector2(350, 320), new Vector2(400, 240));
                b.AddCubicBezier(new Vector2(450, 200), new Vector2(500, 210), new Vector2(540, 250));
                b.AddQuadraticBezier(new Vector2(560, 290), new Vector2(540, 340));
                b.AddLine(new Vector2(470, 380));
                b.AddCubicBezier(new Vector2(450, 440), new Vector2(445, 480), new Vector2(450, 520));
            }), w);
            // muzzle + nostril + eye
            Stroke(ds, Ellipse(rc, 540, 330, 36, 26), w * 0.8f);
            Dot(ds, 552, 326, 5);
            Dot(ds, 492, 278, 9);
            Stroke(ds, Circle(rc, 492, 278, 20), w * 0.6f);
            for (int i = 0; i < 3; i++)
                Line(ds, 506, 262 + i * 6, 522, 258 + i * 6, w * 0.45f);
            // horn spiral
            Stroke(ds, ClosedPoly(rc, new Vector2(500, 218), new Vector2(560, 130), new Vector2(528, 226)), w);
            for (int i = 0; i < 3; i++)
                Line(ds, 512 + i * 10, 204 - i * 22, 530 + i * 6, 210 - i * 22, w * 0.5f);
            // ears
            Stroke(ds, ClosedPoly(rc, new Vector2(452, 224), new Vector2(440, 150), new Vector2(488, 208)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(452, 212), new Vector2(448, 172), new Vector2(470, 204)), w * 0.55f);
            // flowing mane locks
            for (int i = 0; i < 5; i++)
            {
                float y0 = 240 + i * 44;
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(400 - (i % 2) * 10, y0);
                    b.AddCubicBezier(new Vector2(330, y0 + 10), new Vector2(310, y0 + 60), new Vector2(350, y0 + 90));
                }), w * 0.8f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(385 - (i % 2) * 8, y0 + 14);
                    b.AddQuadraticBezier(new Vector2(350, y0 + 40), new Vector2(352, y0 + 70));
                }), w * 0.45f);
            }
            // sparkles + flowers
            foreach (var s in new Vector2[] { new(180, 150), new(620, 140), new(680, 260), new(120, 300) })
            {
                Line(ds, s.X - 14, s.Y, s.X + 14, s.Y, w * 0.55f);
                Line(ds, s.X, s.Y - 14, s.X, s.Y + 14, w * 0.55f);
                Dot(ds, s.X, s.Y, 4);
            }
            foreach (var f in new Vector2[] { new(250, 530), new(550, 535) })
            {
                for (int p = 0; p < 5; p++)
                {
                    float a = p * 72f * (float)Math.PI / 180f;
                    Stroke(ds, Ellipse(rc, f.X + 14 * (float)Math.Cos(a), f.Y + 14 * (float)Math.Sin(a), 10, 7), w * 0.5f);
                }
                Dot(ds, f.X, f.Y, 6);
            }
            Line(ds, 140, 552, 660, 552, w * 0.7f);
        }

        private static void DrawMermaid(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // sun + gulls + distant sail
            Stroke(ds, Circle(rc, 140, 110, 36), w * 0.75f);
            foreach (var g in new Vector2[] { new(520, 110), new(580, 130), new(460, 140) })
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(g.X - 22, g.Y);
                    b.AddQuadraticBezier(new Vector2(g.X - 8, g.Y - 14), new Vector2(g.X, g.Y));
                    b.AddQuadraticBezier(new Vector2(g.X + 8, g.Y - 14), new Vector2(g.X + 22, g.Y));
                }), w * 0.55f);
            // rock layers
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(250, 500);
                b.AddQuadraticBezier(new Vector2(320, 420), new Vector2(400, 430));
                b.AddQuadraticBezier(new Vector2(500, 440), new Vector2(560, 500));
                b.AddLine(new Vector2(250, 500));
            }, close: true), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(280, 470);
                b.AddQuadraticBezier(new Vector2(400, 450), new Vector2(520, 472));
            }), w * 0.55f);
            for (int i = 0; i < 4; i++)
                Line(ds, 300 + i * 60, 486, 310 + i * 60, 500, w * 0.4f);
            // tail with scales
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(400, 430);
                b.AddCubicBezier(new Vector2(360, 360), new Vector2(330, 320), new Vector2(280, 300));
                b.AddCubicBezier(new Vector2(340, 290), new Vector2(380, 330), new Vector2(410, 380));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(410, 430);
                b.AddCubicBezier(new Vector2(440, 360), new Vector2(470, 330), new Vector2(530, 320));
                b.AddCubicBezier(new Vector2(480, 330), new Vector2(440, 370), new Vector2(420, 400));
            }), w);
            for (int r = 0; r < 4; r++)
                for (int c2 = 0; c2 < 3; c2++)
                {
                    float x = 368 + c2 * 28 + (r % 2) * 10;
                    float y = 360 + r * 20;
                    Stroke(ds, Path(rc, b =>
                    {
                        b.BeginFigure(x - 10, y);
                        b.AddQuadraticBezier(new Vector2(x, y + 10), new Vector2(x + 10, y));
                    }), w * 0.45f);
                }
            // tail flukes
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(280, 300);
                b.AddQuadraticBezier(new Vector2(220, 270), new Vector2(190, 220));
                b.AddQuadraticBezier(new Vector2(230, 230), new Vector2(250, 260));
                b.AddQuadraticBezier(new Vector2(260, 220), new Vector2(290, 190));
                b.AddQuadraticBezier(new Vector2(295, 240), new Vector2(310, 295));
            }), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(530, 320);
                b.AddQuadraticBezier(new Vector2(590, 300), new Vector2(620, 250));
                b.AddQuadraticBezier(new Vector2(580, 270), new Vector2(560, 295));
                b.AddQuadraticBezier(new Vector2(555, 255), new Vector2(540, 230));
                b.AddQuadraticBezier(new Vector2(530, 275), new Vector2(520, 315));
            }), w);
            // torso + arms + head
            Stroke(ds, Ellipse(rc, 405, 300, 42, 55), w);
            // shell top
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(385, 285);
                b.AddQuadraticBezier(new Vector2(392, 270), new Vector2(400, 285));
                b.AddQuadraticBezier(new Vector2(408, 270), new Vector2(415, 285));
            }), w * 0.6f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(360, 300);
                b.AddQuadraticBezier(new Vector2(320, 280), new Vector2(290, 240));
            }), w * 0.85f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(450, 300);
                b.AddQuadraticBezier(new Vector2(490, 270), new Vector2(520, 220));
            }), w * 0.85f);
            Stroke(ds, Circle(rc, 405, 200, 42), w);
            Dot(ds, 390, 195, 5); Dot(ds, 420, 195, 5);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(394, 216);
                b.AddQuadraticBezier(new Vector2(405, 224), new Vector2(416, 216));
            }), w * 0.6f);
            // long hair
            for (int i = 0; i < 4; i++)
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(370 + i * 22, 170);
                    b.AddCubicBezier(new Vector2(350 + i * 22, 230), new Vector2(360 + i * 20, 300), new Vector2(340 + i * 26, 360));
                }), w * (0.7f - i * 0.06f));
            Dot(ds, 360, 180, 6);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(452, 240);
                b.AddQuadraticBezier(new Vector2(470, 260), new Vector2(462, 285));
            }), w * 0.55f);
            // waves + bubbles
            for (int row = 0; row < 2; row++)
            {
                float y = 528 + row * 24;
                var pts = new List<Vector2> { new(60, y) };
                for (int i = 0; i < 9; i++)
                {
                    float x0 = 80 + i * 72;
                    pts.Add(new Vector2(x0, y - 12));
                    pts.Add(new Vector2(x0 + 36, y));
                }
                Stroke(ds, Poly(rc, pts.ToArray()), w * 0.65f);
            }
            Stroke(ds, Circle(rc, 620, 380, 14), w * 0.55f);
            Stroke(ds, Circle(rc, 645, 340, 10), w * 0.55f);
        }

        private static void DrawFairyHouse(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // cap
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(180, 300);
                b.AddCubicBezier(new Vector2(220, 130), new Vector2(580, 130), new Vector2(620, 300));
                b.AddQuadraticBezier(new Vector2(400, 340), new Vector2(180, 300));
            }, close: true), w);
            foreach (var s in new (float x, float y, float r)[] { (320, 210, 28), (400, 180, 34), (480, 215, 24), (260, 260, 16), (540, 265, 18), (370, 260, 12) })
                Stroke(ds, Circle(rc, s.x, s.y, s.r), w * 0.65f);
            // gills
            for (int i = 0; i < 9; i++)
                Line(ds, 240 + i * 40, 312, 240 + i * 40, 328, w * 0.45f);
            // stem body with bricks
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(280, 328);
                b.AddLine(new Vector2(270, 520));
                b.AddLine(new Vector2(530, 520));
                b.AddLine(new Vector2(520, 328));
            }), w);
            for (int r = 0; r < 4; r++)
            {
                float y = 370 + r * 38;
                Line(ds, 276, y, 524, y, w * 0.4f);
                for (int c2 = 0; c2 < 5; c2++)
                    Line(ds, 300 + c2 * 46 + (r % 2) * 20, y, 300 + c2 * 46 + (r % 2) * 20, y + 38, w * 0.35f);
            }
            // door
            Stroke(ds, RoundedRect(rc, 360, 410, 80, 110, 36), w);
            Line(ds, 400, 414, 400, 520, w * 0.55f);
            for (int i = 0; i < 2; i++)
                Line(ds, 368, 440 + i * 30, 432, 440 + i * 30, w * 0.4f);
            Dot(ds, 388, 472, 5);
            Stroke(ds, Ellipse(rc, 400, 532, 54, 10), w * 0.6f);
            // round windows + flower boxes
            foreach (float wx in new float[] { 310, 490 })
            {
                Stroke(ds, Circle(rc, wx, 390, 30), w * 0.85f);
                Line(ds, wx - 30, 390, wx + 30, 390, w * 0.55f);
                Line(ds, wx, 360, wx, 420, w * 0.55f);
                Stroke(ds, RoundedRect(rc, wx - 34, 424, 68, 22, 4), w * 0.6f);
                for (int f = 0; f < 3; f++)
                    Dot(ds, wx - 18 + f * 18, 418, 5);
            }
            // chimney + smoke
            Stroke(ds, RoundedRect(rc, 510, 180, 44, 90, 4), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(532, 175);
                b.AddCubicBezier(new Vector2(510, 140), new Vector2(555, 115), new Vector2(535, 80));
                b.AddCubicBezier(new Vector2(520, 55), new Vector2(555, 40), new Vector2(548, 20));
            }), w * 0.6f);
            // vine + stones + flowers
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(270, 520);
                b.AddQuadraticBezier(new Vector2(260, 420), new Vector2(300, 360));
            }), w * 0.6f);
            for (int i = 0; i < 4; i++)
                Stroke(ds, Leaf(rc, new Vector2(272 - i * 4, 490 - i * 32), new Vector2(240 - i * 4, 470 - i * 32), 12), w * 0.55f);
            foreach (var st in new Vector2[] { new(200, 548), new(280, 556), new(370, 556), new(460, 556), new(550, 548) })
                Stroke(ds, Ellipse(rc, st.X, st.Y, 30, 10), w * 0.55f);
            foreach (var f in new Vector2[] { new(150, 480), new(650, 480) })
            {
                Line(ds, f.X, f.Y + 12, f.X, f.Y + 52, w * 0.55f);
                for (int p = 0; p < 5; p++)
                {
                    float a = p * 72f * (float)Math.PI / 180f;
                    Stroke(ds, Ellipse(rc, f.X + 12 * (float)Math.Cos(a), f.Y + 12 * (float)Math.Sin(a), 9, 6), w * 0.5f);
                }
            }
        }

        private static void DrawBalloons(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            void Balloon(float cx, float cy, float rx, float ry, int style)
            {
                Stroke(ds, Ellipse(rc, cx, cy, rx, ry), w);
                // panels
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(cx, cy - ry);
                    b.AddQuadraticBezier(new Vector2(cx - rx * 0.5f, cy), new Vector2(cx, cy + ry));
                    b.AddQuadraticBezier(new Vector2(cx + rx * 0.5f, cy), new Vector2(cx, cy - ry));
                }), w * 0.6f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(cx - rx * 0.7f, cy - ry * 0.4f);
                    b.AddQuadraticBezier(new Vector2(cx - rx * 0.55f, cy + ry * 0.4f), new Vector2(cx - rx * 0.3f, cy + ry * 0.92f));
                }), w * 0.5f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(cx + rx * 0.7f, cy - ry * 0.4f);
                    b.AddQuadraticBezier(new Vector2(cx + rx * 0.55f, cy + ry * 0.4f), new Vector2(cx + rx * 0.3f, cy + ry * 0.92f));
                }), w * 0.5f);
                if (style == 0)
                {
                    for (int i = -1; i <= 1; i++)
                        Line(ds, cx - rx * 0.8f, cy + i * 26, cx + rx * 0.8f, cy + i * 26, w * 0.45f);
                }
                else if (style == 1)
                {
                    for (int i = 0; i < 4; i++)
                        Dot(ds, cx - 30 + (i % 2) * 60, cy - 30 + i * 22, 7);
                    Stroke(ds, Circle(rc, cx, cy, 18), w * 0.55f);
                }
                else
                {
                    Stroke(ds, Poly(rc, new Vector2(cx - rx + 12, cy - 10), new Vector2(cx, cy + 18), new Vector2(cx + rx - 12, cy - 10)), w * 0.5f);
                }
                // ropes + basket
                float by = cy + ry;
                Line(ds, cx - rx * 0.3f, by, cx - 22, by + 62, w * 0.6f);
                Line(ds, cx + rx * 0.3f, by, cx + 22, by + 62, w * 0.6f);
                Line(ds, cx, by, cx, by + 62, w * 0.45f);
                Stroke(ds, ClosedPoly(rc, new Vector2(cx - 28, by + 62), new Vector2(cx + 28, by + 62), new Vector2(cx + 20, by + 96), new Vector2(cx - 20, by + 96)), w * 0.85f);
                Line(ds, cx - 28, by + 78, cx + 28, by + 78, w * 0.45f);
                Line(ds, cx - 14, by + 62, cx - 10, by + 96, w * 0.4f);
                Line(ds, cx + 14, by + 62, cx + 10, by + 96, w * 0.4f);
            }
            Balloon(230, 210, 95, 110, 0);
            Balloon(480, 170, 110, 125, 1);
            Balloon(670, 260, 70, 82, 2);
            Balloon(350, 360, 55, 64, 2);
            Balloon(580, 380, 48, 56, 0);
            // clouds + birds + ground houses
            foreach (var cl in new Vector2[] { new(130, 420), new(680, 470) })
            {
                Stroke(ds, Circle(rc, cl.X, cl.Y, 24), w * 0.55f);
                Stroke(ds, Circle(rc, cl.X + 30, cl.Y - 8, 30), w * 0.55f);
                Stroke(ds, Circle(rc, cl.X + 60, cl.Y, 22), w * 0.55f);
            }
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(380, 90);
                b.AddQuadraticBezier(new Vector2(396, 78), new Vector2(412, 90));
            }), w * 0.5f);
            Stroke(ds, RoundedRect(rc, 300, 500, 90, 60, 4), w * 0.7f);
            Stroke(ds, ClosedPoly(rc, new Vector2(290, 500), new Vector2(345, 460), new Vector2(400, 500)), w * 0.7f);
            Stroke(ds, ClosedPoly(rc, new Vector2(500, 240), new Vector2(520, 200), new Vector2(540, 240)), w * 0.5f);
        }

        private static void DrawDreamcatcher(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(400, 250);
            Stroke(ds, Circle(rc, c.X, c.Y, 150), w);
            Stroke(ds, Circle(rc, c.X, c.Y, 136), w * 0.7f);
            // web: spokes + sagging rings
            int spokes = 16;
            for (int i = 0; i < spokes; i++)
            {
                float a = i * 360f / spokes * (float)Math.PI / 180f;
                Line(ds, c.X + 22 * (float)Math.Cos(a), c.Y + 22 * (float)Math.Sin(a), c.X + 136 * (float)Math.Cos(a), c.Y + 136 * (float)Math.Sin(a), w * 0.45f);
            }
            foreach (float r in new float[] { 110, 82, 54 })
            {
                for (int i = 0; i < spokes; i++)
                {
                    float a0 = i * 360f / spokes * (float)Math.PI / 180f;
                    float a1 = (i + 1) * 360f / spokes * (float)Math.PI / 180f;
                    var p0 = c + new Vector2(r * (float)Math.Cos(a0), r * (float)Math.Sin(a0));
                    var p1 = c + new Vector2(r * (float)Math.Cos(a1), r * (float)Math.Sin(a1));
                    var mid = (p0 + p1) / 2f;
                    var inward = c + Vector2.Normalize(mid - c) * (r - 16);
                    Stroke(ds, Path(rc, b =>
                    {
                        b.BeginFigure(p0.X, p0.Y);
                        b.AddQuadraticBezier(inward, p1);
                    }), w * 0.45f);
                }
            }
            Stroke(ds, Circle(rc, c.X, c.Y, 22), w * 0.8f);
            Dot(ds, c.X, c.Y, 6);
            // top loop + beads on rim
            Stroke(ds, Ellipse(rc, 400, 78, 24, 20), w * 0.8f);
            for (int i = 0; i < 8; i++)
            {
                float a = (-90 + i * 22f) * (float)Math.PI / 180f;
                Dot(ds, c.X + 143 * (float)Math.Cos(a), c.Y + 143 * (float)Math.Sin(a), 5);
            }
            // hanging strands with feathers
            float[] hx = { 300, 400, 500 };
            foreach (float hx0 in hx)
            {
                float topY = 386 + Math.Abs(hx0 - 400) * 0.25f;
                Line(ds, hx0, topY, hx0, topY + 70, w * 0.6f);
                foreach (float by in new float[] { topY + 26, topY + 44 })
                    Stroke(ds, Circle(rc, hx0, by, 10), w * 0.6f);
                var tip = new Vector2(hx0, topY + 160);
                var bas = new Vector2(hx0, topY + 70);
                Stroke(ds, Leaf(rc, bas, tip, 30), w * 0.85f);
                Line(ds, bas.X, bas.Y, tip.X, tip.Y, w * 0.5f);
                for (int k = 1; k <= 4; k++)
                {
                    float t = k / 5f;
                    float y = bas.Y + (tip.Y - bas.Y) * t;
                    Line(ds, hx0, y, hx0 - 22 * (1 - t * 0.5f), y - 12, w * 0.4f);
                    Line(ds, hx0, y, hx0 + 22 * (1 - t * 0.5f), y - 12, w * 0.4f);
                }
            }
        }

        private static void DrawFerris(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(360, 280);
            float R = 170;
            Stroke(ds, Circle(rc, c.X, c.Y, R), w);
            Stroke(ds, Circle(rc, c.X, c.Y, R - 22), w * 0.6f);
            Stroke(ds, Circle(rc, c.X, c.Y, 26), w);
            Stroke(ds, Circle(rc, c.X, c.Y, 10), w * 0.7f);
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * (float)Math.PI / 180f;
                var p = c + new Vector2(R * (float)Math.Cos(a), R * (float)Math.Sin(a));
                Line(ds, c.X, c.Y, p.X, p.Y, w * 0.55f);
                Dot(ds, p.X, p.Y, 5);
            }
            // 8 gondolas
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * (float)Math.PI / 180f;
                var p = c + new Vector2(R * (float)Math.Cos(a), R * (float)Math.Sin(a));
                Line(ds, p.X, p.Y, p.X, p.Y + 28, w * 0.6f);
                Stroke(ds, RoundedRect(rc, p.X - 30, p.Y + 28, 60, 44, 8), w * 0.85f);
                Stroke(ds, RoundedRect(rc, p.X - 20, p.Y + 36, 40, 20, 4), w * 0.55f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(p.X - 30, p.Y + 28);
                    b.AddLine(new Vector2(p.X - 20, p.Y + 12));
                    b.AddLine(new Vector2(p.X + 20, p.Y + 12));
                    b.AddLine(new Vector2(p.X + 30, p.Y + 28));
                }), w * 0.55f);
            }
            // A-frame supports + base
            Line(ds, c.X, c.Y, 250, 540, w);
            Line(ds, c.X, c.Y, 470, 540, w);
            Line(ds, 285, 470, 435, 470, w * 0.6f);
            Line(ds, 305, 505, 415, 505, w * 0.6f);
            Line(ds, 220, 540, 500, 540, w);
            // ticket booth
            Stroke(ds, RoundedRect(rc, 560, 420, 130, 120, 6), w);
            for (int i = 0; i < 5; i++)
                Stroke(ds, ClosedPoly(rc, new Vector2(560 + i * 26, 420), new Vector2(573 + i * 26, 392), new Vector2(586 + i * 26, 420)), w * 0.6f);
            Stroke(ds, RoundedRect(rc, 590, 460, 70, 44, 4), w * 0.7f);
            Line(ds, 625, 460, 625, 504, w * 0.5f);
            Dot(ds, 592, 442, 4);
            // bunting + clouds
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(90, 120);
                b.AddQuadraticBezier(new Vector2(250, 170), new Vector2(420, 120));
            }), w * 0.6f);
            for (int i = 1; i <= 7; i++)
            {
                float t = i / 8f;
                float x = 90 + (420 - 90) * t;
                float y = 120 + (170 - 120) * 4 * t * (1 - t) * 1.6f;
                Stroke(ds, ClosedPoly(rc, new Vector2(x - 12, y), new Vector2(x + 12, y), new Vector2(x, y + 22)), w * 0.55f);
            }
            Stroke(ds, Circle(rc, 660, 120, 26), w * 0.6f);
            Stroke(ds, Circle(rc, 694, 112, 32), w * 0.6f);
        }

        private static void DrawLighthouse(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // beams
            Stroke(ds, ClosedPoly(rc, new Vector2(400, 150), new Vector2(180, 90), new Vector2(180, 190)), w * 0.6f);
            Stroke(ds, ClosedPoly(rc, new Vector2(400, 150), new Vector2(620, 90), new Vector2(620, 190)), w * 0.6f);
            for (int i = 0; i < 3; i++)
                Line(ds, 380 - i * 50, 140 - i * 12, 380 - i * 50, 170 - i * 4, w * 0.4f);
            // tower tapered + stripes
            Stroke(ds, ClosedPoly(rc, new Vector2(350, 200), new Vector2(450, 200), new Vector2(470, 500), new Vector2(330, 500)), w);
            Line(ds, 344, 280, 456, 280, w * 0.7f);
            Line(ds, 338, 360, 462, 360, w * 0.7f);
            Line(ds, 332, 440, 468, 440, w * 0.7f);
            // brick ticks on stripes
            for (int r = 0; r < 3; r++)
            {
                float y = 315 + r * 80;
                for (int c2 = 0; c2 < 4; c2++)
                    Line(ds, 352 + c2 * 26, y, 352 + c2 * 26, y + 30, w * 0.35f);
            }
            // gallery + lamp room + dome
            Line(ds, 330, 200, 470, 200, w);
            for (int i = 0; i < 9; i++)
                Line(ds, 336 + i * 16, 178, 336 + i * 16, 200, w * 0.55f);
            Line(ds, 330, 178, 470, 178, w * 0.8f);
            Stroke(ds, RoundedRect(rc, 356, 110, 88, 68, 6), w);
            Line(ds, 400, 110, 400, 178, w * 0.6f);
            Line(ds, 356, 144, 444, 144, w * 0.55f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(348, 110);
                b.AddQuadraticBezier(new Vector2(400, 60), new Vector2(452, 110));
            }), w);
            Dot(ds, 400, 58, 6);
            Line(ds, 400, 52, 400, 36, w * 0.7f);
            // door + portholes
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(378, 500);
                b.AddLine(new Vector2(378, 450));
                b.AddQuadraticBezier(new Vector2(400, 432), new Vector2(422, 450));
                b.AddLine(new Vector2(422, 500));
            }), w);
            Dot(ds, 412, 476, 4);
            foreach (float py in new float[] { 320, 400 })
            {
                Stroke(ds, Circle(rc, 400, py, 16), w * 0.7f);
                Line(ds, 384, py, 416, py, w * 0.45f);
            }
            // rocks + waves + gulls + boat
            Stroke(ds, ClosedPoly(rc, new Vector2(250, 510), new Vector2(310, 470), new Vector2(370, 505), new Vector2(340, 525)), w * 0.8f);
            Stroke(ds, ClosedPoly(rc, new Vector2(470, 508), new Vector2(530, 472), new Vector2(580, 512)), w * 0.8f);
            for (int row = 0; row < 2; row++)
            {
                float y = 534 + row * 24;
                var pts = new List<Vector2> { new(70, y) };
                for (int i = 0; i < 9; i++)
                {
                    float x0 = 90 + i * 72;
                    pts.Add(new Vector2(x0, y - 12));
                    pts.Add(new Vector2(x0 + 36, y));
                }
                Stroke(ds, Poly(rc, pts.ToArray()), w * 0.65f);
            }
            foreach (var g in new Vector2[] { new(200, 120), new(600, 260) })
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(g.X - 20, g.Y);
                    b.AddQuadraticBezier(new Vector2(g.X - 7, g.Y - 12), new Vector2(g.X, g.Y));
                    b.AddQuadraticBezier(new Vector2(g.X + 7, g.Y - 12), new Vector2(g.X + 20, g.Y));
                }), w * 0.55f);
            Stroke(ds, ClosedPoly(rc, new Vector2(620, 420), new Vector2(620, 360), new Vector2(680, 420)), w * 0.6f);
            Line(ds, 620, 420, 660, 420, w * 0.6f);
        }

        private static void DrawVillage(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // mountains + snow
            Stroke(ds, ClosedPoly(rc, new Vector2(60, 420), new Vector2(220, 160), new Vector2(380, 420)), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(300, 420), new Vector2(500, 140), new Vector2(700, 420)), w);
            Stroke(ds, Poly(rc, new Vector2(185, 220), new Vector2(205, 250), new Vector2(228, 228), new Vector2(250, 258), new Vector2(268, 232)), w * 0.7f);
            Stroke(ds, Poly(rc, new Vector2(455, 210), new Vector2(478, 242), new Vector2(502, 218), new Vector2(526, 250), new Vector2(548, 222)), w * 0.7f);
            // sun + clouds + birds
            Stroke(ds, Circle(rc, 690, 90, 30), w * 0.7f);
            Stroke(ds, Circle(rc, 120, 100, 22), w * 0.55f);
            Stroke(ds, Circle(rc, 150, 92, 28), w * 0.55f);
            // chalets
            void Chalet(float x, float y, float cw, float ch)
            {
                Stroke(ds, RoundedRect(rc, x, y, cw, ch, 4), w);
                Stroke(ds, ClosedPoly(rc, new Vector2(x - 18, y), new Vector2(x + cw / 2, y - 60), new Vector2(x + cw + 18, y)), w);
                Line(ds, x, y + ch * 0.45f, x + cw, y + ch * 0.45f, w * 0.55f);
                for (int i = 0; i < 3; i++)
                    Line(ds, x + 20 + i * 30, y + 12, x + 20 + i * 30, y + ch * 0.45f, w * 0.4f);
                Stroke(ds, RoundedRect(rc, x + cw / 2 - 22, y + ch * 0.55f, 44, ch * 0.45f, 4), w * 0.75f);
                Stroke(ds, RoundedRect(rc, x + 14, y + ch * 0.55f, 34, 30, 3), w * 0.6f);
                Stroke(ds, RoundedRect(rc, x + cw - 48, y + ch * 0.55f, 34, 30, 3), w * 0.6f);
                Line(ds, x + 31, y + ch * 0.55f, x + 31, y + ch * 0.55f + 30, w * 0.4f);
            }
            Chalet(150, 420, 150, 100);
            Chalet(480, 430, 130, 90);
            // church
            Stroke(ds, RoundedRect(rc, 330, 400, 110, 120, 4), w);
            Stroke(ds, ClosedPoly(rc, new Vector2(320, 400), new Vector2(385, 340), new Vector2(450, 400)), w);
            Stroke(ds, RoundedRect(rc, 368, 270, 34, 90, 4), w * 0.85f);
            Stroke(ds, ClosedPoly(rc, new Vector2(360, 270), new Vector2(385, 230), new Vector2(410, 270)), w * 0.85f);
            Line(ds, 385, 212, 385, 230, w * 0.7f);
            Line(ds, 376, 220, 394, 220, w * 0.7f);
            Stroke(ds, Circle(rc, 385, 380, 18), w * 0.7f);
            Line(ds, 385, 362, 385, 398, w * 0.5f);
            Line(ds, 367, 380, 403, 380, w * 0.5f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(360, 520);
                b.AddLine(new Vector2(360, 470));
                b.AddQuadraticBezier(new Vector2(385, 450), new Vector2(410, 470));
                b.AddLine(new Vector2(410, 520));
            }), w * 0.75f);
            // pines + river + bridge + fences
            foreach (float px in new float[] { 90, 660, 610 })
            {
                Stroke(ds, ClosedPoly(rc, new Vector2(px, 380), new Vector2(px - 30, 430), new Vector2(px + 30, 430)), w * 0.7f);
                Stroke(ds, ClosedPoly(rc, new Vector2(px, 410), new Vector2(px - 36, 468), new Vector2(px + 36, 468)), w * 0.7f);
                Line(ds, px, 468, px, 492, w * 0.7f);
            }
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(385, 522);
                b.AddCubicBezier(new Vector2(360, 552), new Vector2(420, 565), new Vector2(400, 590));
            }), w * 0.7f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(300, 556);
                b.AddQuadraticBezier(new Vector2(385, 536), new Vector2(470, 556));
            }), w * 0.7f);
            for (int i = 0; i < 4; i++)
            {
                Line(ds, 120 + i * 30, 500, 120 + i * 30, 522, w * 0.55f);
                Line(ds, 112 + i * 30, 508, 150 + i * 30, 508, w * 0.5f);
            }
        }

        private static void DrawSpace(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var rnd = new Random(99);
            // stars + sparkles
            for (int i = 0; i < 34; i++)
            {
                float x = 60 + rnd.Next(0, 680);
                float y = 40 + rnd.Next(0, 520);
                if (i % 6 == 0)
                {
                    Line(ds, x - 12, y, x + 12, y, w * 0.45f);
                    Line(ds, x, y - 12, x, y + 12, w * 0.45f);
                    Stroke(ds, Circle(rc, x, y, 5), w * 0.45f);
                }
                else Dot(ds, x, y, 3.5f);
            }
            // big ringed planet
            Stroke(ds, Circle(rc, 520, 220, 110), w);
            Stroke(ds, Ellipse(rc, 520, 220, 175, 52), w);
            Stroke(ds, Ellipse(rc, 520, 220, 155, 42), w * 0.6f);
            foreach (var cr in new (float x, float y, float r)[] { (480, 190, 18), (540, 240, 24), (560, 190, 12), (500, 255, 10) })
                Stroke(ds, Circle(rc, cr.x, cr.y, cr.r), w * 0.55f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(430, 250);
                b.AddQuadraticBezier(new Vector2(480, 268), new Vector2(530, 256));
            }), w * 0.5f);
            // moon
            Stroke(ds, Circle(rc, 170, 180, 60), w);
            foreach (var mc in new (float x, float y, float r)[] { (150, 165, 12), (180, 195, 16), (165, 205, 8) })
                Stroke(ds, Circle(rc, mc.x, mc.y, mc.r), w * 0.55f);
            // rocket
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(300, 200);
                b.AddQuadraticBezier(new Vector2(330, 160), new Vector2(360, 200));
                b.AddLine(new Vector2(360, 320));
                b.AddLine(new Vector2(300, 320));
            }, close: true), w);
            Stroke(ds, Circle(rc, 330, 244, 22), w * 0.8f);
            Stroke(ds, Circle(rc, 330, 244, 10), w * 0.55f);
            Stroke(ds, ClosedPoly(rc, new Vector2(300, 290), new Vector2(268, 340), new Vector2(300, 330)), w * 0.85f);
            Stroke(ds, ClosedPoly(rc, new Vector2(360, 290), new Vector2(392, 340), new Vector2(360, 330)), w * 0.85f);
            for (int i = 0; i < 3; i++)
                Line(ds, 310 + i * 20, 320, 306 + i * 20, 352, w * 0.6f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(312, 360);
                b.AddQuadraticBezier(new Vector2(330, 390), new Vector2(348, 360));
                b.AddQuadraticBezier(new Vector2(340, 380), new Vector2(330, 395));
                b.AddQuadraticBezier(new Vector2(320, 380), new Vector2(312, 360));
            }), w * 0.6f);
            // UFO
            Stroke(ds, Ellipse(rc, 250, 450, 90, 30), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(200, 442);
                b.AddQuadraticBezier(new Vector2(250, 390), new Vector2(300, 442));
            }), w);
            foreach (float lx in new float[] { 200, 232, 264, 296 })
            {
                Stroke(ds, Circle(rc, lx, 452, 9), w * 0.6f);
                Dot(ds, lx, 452, 3);
            }
            Line(ds, 232, 478, 220, 500, w * 0.55f);
            Line(ds, 268, 478, 280, 500, w * 0.55f);
            // shooting star + constellation
            Line(ds, 90, 420, 200, 360, w * 0.7f);
            Line(ds, 110, 424, 200, 372, w * 0.45f);
            Dot(ds, 200, 360, 6);
            var cons = new Vector2[] { new(560, 420), new(600, 450), new(640, 430), new(620, 480), new(570, 470) };
            Stroke(ds, Poly(rc, cons), w * 0.5f);
            foreach (var p in cons) Stroke(ds, Circle(rc, p.X, p.Y, 7), w * 0.6f);
        }

        private static void DrawCityNight(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            // moon + stars
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(660, 70);
                b.AddQuadraticBezier(new Vector2(615, 105), new Vector2(660, 145));
                b.AddQuadraticBezier(new Vector2(632, 105), new Vector2(660, 70));
            }, close: true), w * 0.8f);
            var rnd = new Random(5);
            for (int i = 0; i < 22; i++)
                Dot(ds, 60 + rnd.Next(0, 500), 40 + rnd.Next(0, 150), 3.5f);
            // buildings with window grids
            var blds = new (float x, float bw, float top)[] { (80, 80, 260), (175, 100, 200), (290, 70, 300), (375, 110, 170), (500, 85, 240), (600, 90, 210) };
            foreach (var b in blds)
            {
                Stroke(ds, RoundedRect(rc, b.x, b.top, b.bw, 340 - b.top, 3), w);
                Line(ds, b.x, b.top + 24, b.x + b.bw, b.top + 24, w * 0.5f);
                int cols = (int)(b.bw / 24);
                int rows = (int)((340 - b.top - 30) / 30);
                for (int r = 0; r < rows; r++)
                    for (int c2 = 0; c2 < cols; c2++)
                    {
                        float wx = b.x + 12 + c2 * 24;
                        float wy = b.top + 40 + r * 30;
                        if ((r + c2) % 4 == 0) { Line(ds, wx - 6, wy - 6, wx + 6, wy + 6, w * 0.35f); Line(ds, wx + 6, wy - 6, wx - 6, wy + 6, w * 0.35f); }
                        else Stroke(ds, RoundedRect(rc, wx - 7, wy - 8, 14, 16, 2), w * 0.45f);
                    }
                if (b.x == 375) { Line(ds, b.x + b.bw / 2, b.top, b.x + b.bw / 2, b.top - 40, w * 0.7f); Dot(ds, b.x + b.bw / 2, b.top - 44, 5); }
            }
            // street + lamps + car + crosswalk
            Line(ds, 60, 420, 740, 420, w);
            Line(ds, 60, 540, 740, 540, w);
            foreach (float lx in new float[] { 150, 400, 650 })
            {
                Line(ds, lx, 420, lx, 520, w * 0.8f);
                Stroke(ds, RoundedRect(rc, lx - 18, 398, 36, 20, 8), w * 0.7f);
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(lx - 12, 418);
                    b.AddLine(new Vector2(lx - 40, 470));
                    b.AddLine(new Vector2(lx + 40, 470));
                    b.AddLine(new Vector2(lx + 12, 418));
                }), w * 0.4f);
            }
            // car
            Stroke(ds, RoundedRect(rc, 250, 460, 150, 44, 16), w);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(280, 460);
                b.AddQuadraticBezier(new Vector2(300, 430), new Vector2(330, 430));
                b.AddLine(new Vector2(370, 430));
                b.AddQuadraticBezier(new Vector2(390, 430), new Vector2(400, 460));
            }), w);
            Line(ds, 340, 430, 340, 460, w * 0.55f);
            Stroke(ds, Circle(rc, 290, 506, 20), w);
            Stroke(ds, Circle(rc, 360, 506, 20), w);
            Stroke(ds, Circle(rc, 290, 506, 7), w * 0.6f);
            Stroke(ds, Circle(rc, 360, 506, 7), w * 0.6f);
            for (int i = 0; i < 4; i++)
                Stroke(ds, RoundedRect(rc, 480 + i * 44, 522, 28, 10, 2), w * 0.5f);
        }

        private static void DrawMandalaSun(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(400, 300);
            Dot(ds, c.X, c.Y, 8);
            Stroke(ds, Circle(rc, c.X, c.Y, 24), w * 0.8f);
            Stroke(ds, Circle(rc, c.X, c.Y, 44), w * 0.8f);
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * (float)Math.PI / 180f;
                Dot(ds, c.X + 34 * (float)Math.Cos(a), c.Y + 34 * (float)Math.Sin(a), 4);
            }
            // inner petals 12
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f;
                var tip = c + Vector2.Transform(new Vector2(0, -110), Matrix3x2.CreateRotation(a * (float)Math.PI / 180f));
                var petalBase = c + Vector2.Transform(new Vector2(0, -44), Matrix3x2.CreateRotation(a * (float)Math.PI / 180f));
                Stroke(ds, Leaf(rc, petalBase, tip, 22), w * 0.8f);
                Line(ds, petalBase.X, petalBase.Y, tip.X, tip.Y, w * 0.4f);
            }
            Stroke(ds, Circle(rc, c.X, c.Y, 116), w);
            Stroke(ds, Circle(rc, c.X, c.Y, 128), w * 0.6f);
            // tick ring 48
            for (int i = 0; i < 48; i++)
            {
                float a = i * 7.5f * (float)Math.PI / 180f;
                var p1 = c + new Vector2(128 * (float)Math.Cos(a), 128 * (float)Math.Sin(a));
                var p2 = c + new Vector2((i % 4 == 0 ? 148 : 140) * (float)Math.Cos(a), (i % 4 == 0 ? 148 : 140) * (float)Math.Sin(a));
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w * 0.45f);
            }
            // diamond ring 16
            for (int i = 0; i < 16; i++)
            {
                float a = i * 22.5f * (float)Math.PI / 180f;
                var ctr = c + new Vector2(172 * (float)Math.Cos(a), 172 * (float)Math.Sin(a));
                var up = ctr + new Vector2(20 * (float)Math.Cos(a), 20 * (float)Math.Sin(a));
                var dn = ctr - new Vector2(20 * (float)Math.Cos(a), 20 * (float)Math.Sin(a));
                var l = ctr + new Vector2(13 * -(float)Math.Sin(a), 13 * (float)Math.Cos(a));
                var r2 = ctr - new Vector2(13 * -(float)Math.Sin(a), 13 * (float)Math.Cos(a));
                Stroke(ds, ClosedPoly(rc, up, l, dn, r2), w * 0.6f);
                Dot(ds, ctr.X, ctr.Y, 3.5f);
            }
            // outer scallop 32 arcs + outer circles
            for (int i = 0; i < 32; i++)
            {
                float a0 = i * 11.25f * (float)Math.PI / 180f;
                float a1 = (i + 1) * 11.25f * (float)Math.PI / 180f;
                var p0 = c + new Vector2(200 * (float)Math.Cos(a0), 200 * (float)Math.Sin(a0));
                var p1 = c + new Vector2(200 * (float)Math.Cos(a1), 200 * (float)Math.Sin(a1));
                var mid = c + new Vector2(216 * (float)Math.Cos((a0 + a1) / 2), 216 * (float)Math.Sin((a0 + a1) / 2));
                Stroke(ds, Path(rc, b =>
                {
                    b.BeginFigure(p0.X, p0.Y);
                    b.AddQuadraticBezier(mid, p1);
                }), w * 0.55f);
            }
            Stroke(ds, Circle(rc, c.X, c.Y, 200), w * 0.8f);
            Stroke(ds, Circle(rc, c.X, c.Y, 212), w);
        }

        private static void DrawMandalaTide(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(400, 300);
            // center spiral rings
            Stroke(ds, Circle(rc, c.X, c.Y, 18), w * 0.8f);
            Stroke(ds, Circle(rc, c.X, c.Y, 36), w * 0.7f);
            Stroke(ds, Path(rc, b =>
            {
                b.BeginFigure(c.X + 18, c.Y);
                b.AddQuadraticBezier(new Vector2(c.X, c.Y - 26), new Vector2(c.X - 18, c.Y));
                b.AddQuadraticBezier(new Vector2(c.X, c.Y + 26), new Vector2(c.X + 18, c.Y));
            }), w * 0.55f);
            // 12 teardrops
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f;
                var bpos = c + Vector2.Transform(new Vector2(0, -40), Matrix3x2.CreateRotation(a * (float)Math.PI / 180f));
                var tip = c + Vector2.Transform(new Vector2(0, -100), Matrix3x2.CreateRotation(a * (float)Math.PI / 180f));
                Stroke(ds, Leaf(rc, bpos, tip, 20), w * 0.75f);
                Dot(ds, (bpos.X + tip.X) / 2, (bpos.Y + tip.Y) / 2, 3.5f);
            }
            Stroke(ds, Circle(rc, c.X, c.Y, 105), w);
            // wave ring: sine poly
            var wave = new List<Vector2>();
            for (int i = 0; i <= 96; i++)
            {
                float a = i * 3.75f * (float)Math.PI / 180f;
                float r = 128 + 12 * (float)Math.Sin(i * 0.65f);
                wave.Add(c + new Vector2(r * (float)Math.Cos(a), r * (float)Math.Sin(a)));
            }
            Stroke(ds, Poly(rc, wave.ToArray()), w * 0.7f);
            // shell fans 12
            for (int i = 0; i < 12; i++)
            {
                float a = (i * 30f + 15) * (float)Math.PI / 180f;
                var ctr = c + new Vector2(168 * (float)Math.Cos(a), 168 * (float)Math.Sin(a));
                Stroke(ds, Circle(rc, ctr.X, ctr.Y, 26), w * 0.65f);
                for (int k = -1; k <= 1; k++)
                {
                    float a2 = a + k * 0.3f;
                    Line(ds, c.X + 140 * (float)Math.Cos(a2), c.Y + 140 * (float)Math.Sin(a2), ctr.X, ctr.Y, w * 0.35f);
                }
                Dot(ds, ctr.X, ctr.Y, 4);
            }
            // rope ring hatches
            Stroke(ds, Circle(rc, c.X, c.Y, 198), w);
            Stroke(ds, Circle(rc, c.X, c.Y, 214), w);
            for (int i = 0; i < 48; i++)
            {
                float a = i * 7.5f * (float)Math.PI / 180f;
                var p1 = c + new Vector2(198 * (float)Math.Cos(a), 198 * (float)Math.Sin(a));
                var p2 = c + new Vector2(214 * (float)Math.Cos(a + 0.08f), 214 * (float)Math.Sin(a + 0.08f));
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w * 0.4f);
            }
            // 4 mini stars around
            foreach (float sa in new float[] { 45, 135, 225, 315 })
            {
                float a = sa * (float)Math.PI / 180f;
                var sc = c + new Vector2(258 * (float)Math.Cos(a), 215 * (float)Math.Sin(a) * 1.15f);
                var pts = new List<Vector2>();
                for (int k = 0; k < 10; k++)
                {
                    float r = (k % 2 == 0) ? 18 : 8;
                    float ka = k * 36f * (float)Math.PI / 180f - (float)Math.PI / 2;
                    pts.Add(sc + new Vector2(r * (float)Math.Cos(ka), r * (float)Math.Sin(ka)));
                }
                Stroke(ds, ClosedPoly(rc, pts.ToArray()), w * 0.55f);
            }
        }

        private static void DrawMandalaStar(CanvasDrawingSession ds, float w)
        {
            var rc = (ICanvasResourceCreator)ds;
            var c = new Vector2(400, 300);
            // inner 8 petals
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f;
                var tip = c + Vector2.Transform(new Vector2(0, -84), Matrix3x2.CreateRotation(a * (float)Math.PI / 180f));
                Stroke(ds, Ellipse(rc, (c.X + tip.X) / 2, (c.Y + tip.Y) / 2, 20, 34), w * 0.65f);
            }
            Stroke(ds, Circle(rc, c.X, c.Y, 30), w * 0.8f);
            Stroke(ds, Circle(rc, c.X, c.Y, 88), w * 0.8f);
            Dot(ds, c.X, c.Y, 7);
            // two overlapping squares (8-point star)
            var sq1 = new Vector2[] { new(c.X - 110, c.Y - 110), new(c.X + 110, c.Y - 110), new(c.X + 110, c.Y + 110), new(c.X - 110, c.Y + 110) };
            Stroke(ds, ClosedPoly(rc, sq1), w * 0.8f);
            var sq2 = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45) * (float)Math.PI / 180f;
                sq2.Add(c + new Vector2(155 * (float)Math.Cos(a), 155 * (float)Math.Sin(a)));
            }
            Stroke(ds, ClosedPoly(rc, sq2.ToArray()), w * 0.8f);
            // dots on square corners + edge mids
            foreach (var p in sq1) Stroke(ds, Circle(rc, p.X, p.Y, 12), w * 0.55f);
            foreach (var p in sq2) Stroke(ds, Circle(rc, p.X, p.Y, 12), w * 0.55f);
            // 16 outer triangles
            for (int i = 0; i < 16; i++)
            {
                float a = i * 22.5f * (float)Math.PI / 180f;
                var tip = c + new Vector2(228 * (float)Math.Cos(a), 228 * (float)Math.Sin(a));
                var l = c + new Vector2(182 * (float)Math.Cos(a - 0.09f), 182 * (float)Math.Sin(a - 0.09f));
                var r2 = c + new Vector2(182 * (float)Math.Cos(a + 0.09f), 182 * (float)Math.Sin(a + 0.09f));
                Stroke(ds, ClosedPoly(rc, tip, l, r2), w * 0.65f);
                Dot(ds, (tip.X + c.X) / 2 + (tip.X - c.X) * 0.08f, (tip.Y + c.Y) / 2 + (tip.Y - c.Y) * 0.08f, 3.5f);
            }
            Stroke(ds, Circle(rc, c.X, c.Y, 182), w * 0.7f);
            Stroke(ds, Circle(rc, c.X, c.Y, 232), w);
            for (int i = 0; i < 64; i++)
            {
                float a = i * 5.625f * (float)Math.PI / 180f;
                var p1 = c + new Vector2(232 * (float)Math.Cos(a), 232 * (float)Math.Sin(a));
                var p2 = c + new Vector2(240 * (float)Math.Cos(a), 240 * (float)Math.Sin(a));
                Line(ds, p1.X, p1.Y, p2.X, p2.Y, w * 0.4f);
            }
        }
    }
}
