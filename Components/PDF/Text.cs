using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Headless.Lib;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Components.PDF
{
    public class Text : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Text class.
        /// </summary>
        public Text()
          : base("Text", "Nickname",
              "Description",
              "Headless", "PDF")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            Point3d pt = Point3d.Unset;
            string text = string.Empty;

            var skPt = new SKPoint();

            skPt.X = Convert.ToSingle(pt.X);
            skPt.Y = Convert.ToSingle(pt.Y);
            var t = new TextBlob
            {
                Position = skPt,
                Text = text,
            };
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
            get { return new Guid("BA08AFA1-DCD2-4365-BD96-865A3AB69291"); }
        }
    }
}