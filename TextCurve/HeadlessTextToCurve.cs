using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using Grasshopper.Kernel.Types;
using System.Linq;
using Grasshopper.Kernel.Parameters;

namespace Headless.TextCurve
{
    public class HeadlessTextToCurve : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public HeadlessTextToCurve()
          : base("HeadlessTextToCurve", "HTTC",
            "This component allows creating text curves in a headless Rhino environment like compute",
            "Headless", "Primitive")
        {
        }


        //public override void DrawViewportWires(IGH_PreviewArgs args)
        //{
        //    //base.DrawViewportMeshes(args);
        //}
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //base.DrawViewportMeshes(args);
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "The text that should be displayed", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Plane of the text default is XY", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "S", "Scale of the font", GH_ParamAccess.item, 10);

            //Setting up the font
            pManager.AddTextParameter("Font", "F", "Custom installed font => Make sure the font of your choosing is installed of the matching that this component is running on otherwise a generic font will be used", GH_ParamAccess.item, "Arial");


            //Text alignment
            Param_Integer hzParam = new Param_Integer();

            hzParam.AddNamedValue("Left", 0);
            hzParam.AddNamedValue("Center", 1);
            hzParam.AddNamedValue("Right", 2);
            hzParam.PersistentData.Append(new GH_Integer(1));

            pManager.AddParameter(hzParam, "Horizontal Alignment", "HA", "Sets the horizontal alignment", GH_ParamAccess.item);

            Param_Integer vertParam = new Param_Integer();

            vertParam.AddNamedValue("Top", 0);
            vertParam.AddNamedValue("Middle", 1);
            vertParam.AddNamedValue("Bottom", 2);
            vertParam.PersistentData.Append(new GH_Integer(1));

            pManager.AddParameter(vertParam, "Vertical Alignment", "VA", "Sets the vertical alignment", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("TextCurves", "TC", "Output curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Declare a variable for each the inputs
            string text = string.Empty;
            double scale = 10;
            Plane plane = Plane.WorldXY;
            string font = "Arial";
            int horzAlign = 0;
            int vertAlign = 0;

            // Retrieve each of the inputs. If any are unsuccessful, return.
            if (!DA.GetData(0, ref text)) { return; }
            if (!DA.GetData(1, ref plane)) { return; }
            if (!DA.GetData(2, ref scale)) { return; }
            if (!DA.GetData(3, ref font)) { return; }
            if (!DA.GetData(4, ref horzAlign)) { return; }
            if (!DA.GetData(5, ref vertAlign)) { return; }


            //Options for horizontal alignment
            TextHorizontalAlignment[] textHorizontalAlignment = new TextHorizontalAlignment[] {
                TextHorizontalAlignment.Right,
                TextHorizontalAlignment.Center,
                TextHorizontalAlignment.Left
            };

            //Options for vertical alignment
            TextVerticalAlignment[] textVerticalAlignment = new TextVerticalAlignment[] {
                TextVerticalAlignment.Bottom,
                TextVerticalAlignment.Middle,
                TextVerticalAlignment.Top
            };

            //Create a headless doc
            var doc = RhinoDoc.CreateHeadless(null);

            //Create an custom dimstyle
            DimensionStyle style = new DimensionStyle();
            style.TextHeight = scale;
            style.TextHorizontalAlignment = textHorizontalAlignment[horzAlign];
            style.TextVerticalAlignment = textVerticalAlignment[vertAlign];

            //Copy plane since if plane is directly applied the text is not properly aligned
            Plane orientationPlane = new Plane(plane);

            //Plane to set the basic location
            plane = Plane.WorldXY;
            plane.Origin = orientationPlane.Origin;

            //Create the transform for transforming the curves in the end
            Transform transform = Transform.PlaneToPlane(plane, orientationPlane);

            //Set the document font
            Font documentFont;
            //Gets all doc fonts
            Font[] fs = Rhino.DocObjects.Font.InstalledFonts(font);

            if (fs.Length != 0)
            {
                //Sets a custom font
                documentFont = fs[0];
            }
            else
            {
                //If the custom font cant be found replace it with a base font => Arial
                documentFont = doc.DimStyles[0].Font;
            }

            //Set the font to the dimstyle
            style.Font = documentFont;

            //Create text entity
            TextEntity te = TextEntity.Create(text, plane, style, false, 1, 0);

            //Convert text entity to curves
            Curve[] curves = te.CreateCurves(style, false, 0, 0);

            //Transform the curve
            foreach (Curve curve in curves)
            {
                curve.Transform(transform);
            }

            //free recourses 
            doc.Dispose();
            te.Dispose();

            //Wrap the curve array in a grasshopper native array
            GH_Curve[] gH_Curves = curves.Select(c => new GH_Curve(c)).ToArray();
            //Output
            DA.SetDataList(0, gH_Curves);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.textCurve;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("955d5c3c-a299-4edc-b90b-6d76c7eabfc5");
    }
}