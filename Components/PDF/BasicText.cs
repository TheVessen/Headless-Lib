using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Components.PDF
{
    public class BasicText : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public BasicText()
          : base("BasicText", "BT",
              "Add text to the page. The difference between text blob is that this text will be not scaled with the paths",
              "Headless", "PDF")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "Text to be written", GH_ParamAccess.item );
            pManager.AddGenericParameter("TextStyle", "TS", "Text Style", GH_ParamAccess.item);
            pManager.AddNumberParameter("TextFromTop", "TFT", "Text From Top", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("TextFromLeft", "TFL", "Text From Left", GH_ParamAccess.item, 0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
                
                pManager.AddGenericParameter("TextElement", "TE", "Text Element", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            string text = string.Empty;
            GH_ObjectWrapper _textStyle = null;
            double textFromTop = 0.0;
            double textFromLeft = 0.0;

            if (!DA.GetData(0, ref text)) return;
            if (!DA.GetData(1, ref _textStyle)) return;
            if (!DA.GetData(2, ref textFromTop)) return;
            if (!DA.GetData(3, ref textFromLeft)) return;
            
            var paint = _textStyle.Value as SKPaint;

            var textElement = new Lib.PdfTextObject(text, paint, textFromTop, textFromLeft);

            DA.SetData(0, textElement);
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
        
        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("E6565124-FE51-4740-A082-23E36575D421"); }
        }
    }
}