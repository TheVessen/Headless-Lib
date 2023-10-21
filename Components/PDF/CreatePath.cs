using System;
using Grasshopper.Kernel;
using Headless.Lib;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Components.PDF
{
    public class CreatePath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatePath class.
        /// </summary>
        public CreatePath()
            : base("CreatePath", "CP",
                "Converts lines and curves to paths",
                "Headless", "PDF")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Input curves", GH_ParamAccess.item);
            pManager.AddGenericParameter("Paint", "P", "The Skia paint properties", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Path", "P", "SkiaPaths", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve curve = null;
            SKPaint paint = null;

            if (!DA.GetData(0, ref curve)) return;
            if (!DA.GetData(1, ref paint)) return;

            SKPath path = null;

            if (curve.GetType() == typeof(NurbsCurve))
            {
                var c = curve as NurbsCurve;
                path = RhCurveToSkia.ConvertNurbsToSkia(c);
            }
            else if (curve.GetType() == typeof(ArcCurve))
            {
                var c = curve as ArcCurve;
                path = RhCurveToSkia.ConvertArcCurveToSkiaPath(c);
            }
            else if (curve.GetType() == typeof(PolyCurve))
            {
                var c = curve as PolyCurve;
                path = RhCurveToSkia.ConvertNurbsToSkia(c);
            }
            else
            {
                var c = curve as PolylineCurve;
                path = RhCurveToSkia.ConvertPolylineToSkiaPath(c);
            }
            
            var outputData = new SkiaCurveData
            {
                Path = path,
                Paint = paint
            };

            DA.SetData(0, outputData);
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
            get { return new Guid("51D18ECF-6230-439C-8BAD-3027F676F740"); }
        }
    }
}