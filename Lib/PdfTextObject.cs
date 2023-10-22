using System;
using SkiaSharp;

namespace Headless.Lib
{
    public class PdfTextObject
    {
        public string Text { get; set; }
        public SKPaint TextStyle { get; set; }
        public float TextFromTop { get; set; }
        public float TextFromLeft { get; set; }
        
        public PdfTextObject(string text, SKPaint textStyle, double textFromTop, double textFromLeft)
        {
            Text = text;
            TextStyle = textStyle;
            TextFromTop = Convert.ToSingle(textFromTop) ;
            TextFromLeft =  Convert.ToSingle(textFromLeft);
        }
        
        public PdfTextObject(string text, SKPaint textStyle, float textFromTop, float textFromLeft)
        {
            Text = text;
            TextStyle = textStyle;
            TextFromTop = textFromTop;
            TextFromLeft = textFromLeft;
        }
        
    }
}