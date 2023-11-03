using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Components.PDF
{
    public class PathStyle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PathStyle class.
        /// </summary>
        public PathStyle()
            : base("PathStyle", "PS",
                "Creates the style for the path",
                "Headless", "PDF")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Round Caps", "R", "Whether to round the caps or not", GH_ParamAccess.item, false); // default to false
            pManager.AddNumberParameter("Stroke Width", "S", "Width of the stroke", GH_ParamAccess.item, 1.0); // default to 1.0
            pManager.AddColourParameter("Color", "C", "Color of the stroke", GH_ParamAccess.item, Color.Gray); // default to Gray
            pManager.AddBooleanParameter("DrawFill", "DR", "Draw fill", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.AddGenericParameter("Paint", "P", "Paint -> for styling the curves", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool roundCaps = false;
            double tempStrokeWidth = 1.0;
            Color color = Color.Gray;
            bool drawFill = false;

            if (!DA.GetData(0, ref roundCaps)) return;
            if (!DA.GetData(1, ref tempStrokeWidth)) return;
            if (!DA.GetData(2, ref color)) return;
            if (!DA.GetData(3, ref drawFill)) return;

            float strokeWidth = (float)tempStrokeWidth;

            var drawing = new SKPaint();

            // Set color
            drawing.Color = new SKColor(color.R, color.G, color.B, color.A);

            // Style Line
            drawing.StrokeCap = roundCaps ? SKStrokeCap.Round : SKStrokeCap.Square;
            drawing.StrokeJoin = roundCaps ? SKStrokeJoin.Round : SKStrokeJoin.Bevel;

            // Set drawing style based on drawFill value
            drawing.Style = drawFill ? SKPaintStyle.Fill : SKPaintStyle.Stroke;

            drawing.StrokeWidth = strokeWidth;

            drawing.IsAntialias = true;

            DA.SetData(0, drawing);
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
        
        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("FD8597C6-A0A3-4766-97F7-35BA241D5F28"); }
        }
    }
}