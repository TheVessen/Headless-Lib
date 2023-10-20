using SkiaSharp;

namespace Headless.Lib
{
    public class TextBlob
    {
        public string Text { get; set; }
        public SKPoint Position { get; set; }
        public SKPaint TextPaint { get; set; }

        public void TransformPosition(SKMatrix transformationMatrix)
        {
            // Apply the transformation matrix to the Position.
            Position = new SKPoint(
                (Position.X * transformationMatrix.ScaleX) + transformationMatrix.TransX,
                (Position.Y * transformationMatrix.ScaleY) + transformationMatrix.TransY
            );
        }
    }
}