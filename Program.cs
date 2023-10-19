using System;
using QuestPDF.Fluent;
using QuestPDF.Previewer;
using SkiaSharp;

namespace Headless
{
    public class Program
    {
        static void Main(string[] args)
        {
            var info = new SKImageInfo(800, 600);

            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;

                canvas.Clear(SKColors.White);

                using (var paint = new SKPaint
                       {
                           Style = SKPaintStyle.Stroke,
                           Color = SKColors.Blue,
                           StrokeWidth = 3
                       })
                using (var path = new SKPath())
                {
                    path.MoveTo(50, 50);
                    path.QuadTo(400, 0, 50, 300);  // control point and end point

                    path.MoveTo(50, 50);
                    path.CubicTo(200, 300, 600, 300, 750, 50);  // two control points and end point

                    canvas.DrawPath(path, paint);

                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string outputPath = System.IO.Path.Combine(desktopPath, "output.png");

                    using (var image = surface.Snapshot())
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                    using (var stream = System.IO.File.OpenWrite(outputPath))
                    {
                        data.SaveTo(stream);
                    }
                }
            }

        }
    }
}