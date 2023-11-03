using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Headless.Components.PDF
{
    public class PageSize : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PageSitze class.
        /// </summary>
        public PageSize()
            : base("PageSize", "Nickname",
                "Description",
                "Headless", "PDF")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("PageFormat", "VL", "Connect a Value List here", GH_ParamAccess.item, "210 x 297");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Height", "H", "Page Height", GH_ParamAccess.item);
            pManager.AddNumberParameter("Width", "W", "Page Width", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string textFormat = "210 x 297";

            if (!DA.GetData(0, ref textFormat)) return;

            string[] dimensions = textFormat.Split('x');
            if (dimensions.Length == 2)
            {
                string widthString = dimensions[0].Trim();
                string heightString = dimensions[1].Trim();

                if (double.TryParse(widthString, out double width) && double.TryParse(heightString, out double height))
                {

                    DA.SetData(0, width);
                    DA.SetData(1, height);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to parse dimensions to doubles.");
                }
            }
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
            get { return new Guid("6F9DE34C-FFE0-4200-816A-051027E21B91"); }
        }
    }
}