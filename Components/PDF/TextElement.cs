using System;
using System.Drawing;
using Grasshopper.Kernel;
using Headless.Lib;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Components.PDF
{
    public class TextElement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TextElement class.
        /// </summary>
        public TextElement()
          : base("TextElement", "Nickname",
              "Description",
              "Headless", "PDF")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to add to the pdf", GH_ParamAccess.item);
            pManager.AddPointParameter("Point", "P", "Position of the text", GH_ParamAccess.item);
            pManager.AddNumberParameter("TextSize", "TS", "Number for text size", GH_ParamAccess.item, 10);
            pManager.AddColourParameter("Color", "C", "Color of the font", GH_ParamAccess.item, Color.Black);
            pManager.AddTextParameter("Font", "F", "Font", GH_ParamAccess.item, "Arial");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("TextBlob", "TB", "Text Blob", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            string text = string.Empty;
            Point3d pt = Point3d.Unset;
            double textSize = 10;
            Color color = Color.Black;
            string font = "Arial";

            if (!DA.GetData(0, ref text)) return;
            if (!DA.GetData(1, ref pt)) return;
            if (!DA.GetData(2, ref textSize)) return;
            if (!DA.GetData(3, ref color)) return;
            if (!DA.GetData(4, ref font)) return;
            
            // Define the text, paint, and position for the positioned run
            SKPaint textPaint = new SKPaint
            {
                TextSize = Convert.ToSingle(textSize),
                Color = new SKColor(color.R, color.G, color.B, color.A),
                Typeface = SKTypeface.FromFamilyName(font),
                TextAlign = SKTextAlign.Center,
                IsAntialias = true,
            };
            
            SKPoint positions = new SKPoint { X = Convert.ToSingle(pt.X), Y = Convert.ToSingle(pt.Y) };
            var tb = new TextBlob()
            {
                Position = positions,
                Text = text,
                TextPaint = textPaint,
            };

            DA.SetData(0, tb);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E042A243-BB77-4707-A688-0A38AADA9FB7"); }
        }
    }
}