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


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "T", "The text that should be dispayed", GH_ParamAccess.item, "Default");
            pManager.AddNumberParameter("Scale", "S", "Scale of the font", GH_ParamAccess.item, 10);
            pManager.AddPointParameter("Location", "L", "Location of the text", GH_ParamAccess.item, new Point3d(0, 0, 0));
            pManager.AddPlaneParameter("Plane", "P", "Plane of the text default is XY", GH_ParamAccess.item, Plane.WorldXY);

            //Setting up the font
            pManager.AddTextParameter("Font", "F", "Custom installed font => Make sure the font of your choosing is installed of the maching that this component is running on otherwhise a generic font will be used", GH_ParamAccess.item, "Arial");


            //Text alignment
            Param_Integer justification = new Param_Integer();

            justification.AddNamedValue("Left", 0);
            justification.AddNamedValue("Center", 1);
            justification.AddNamedValue("Right", 2);
            justification.PersistentData.Append(new GH_Integer(0));

            pManager.AddParameter(justification, "Text Justification", "TJ", "List of text justification options", GH_ParamAccess.item);

            Param_Integer ajudtParam = new Param_Integer();

            ajudtParam.AddNamedValue("Top", 0);
            ajudtParam.AddNamedValue("Middle", 1);
            ajudtParam.AddNamedValue("Bottom", 2);
            ajudtParam.Name = "Text Alignment";
            ajudtParam.NickName = "TA";
            ajudtParam.PersistentData.Append(new GH_Integer(0));

            pManager.AddParameter(ajudtParam, "Text Alignment", "TA", "Set of adjustment options" , GH_ParamAccess.item);

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
            string text = "Default";
            double scale = 10;
            Point3d location = Point3d.Unset;
            Plane plane = Plane.WorldXY;
            string font = "Arial";
            int justification = 0;
            int alignment = 0;

            // Retrieve each of the inputs. If any are unsuccessful, return.
            if (!DA.GetData(0, ref text)) { return; }
            if (!DA.GetData(1, ref scale)) { return; }
            if (!DA.GetData(2, ref location)) { return; }
            if (!DA.GetData(3, ref plane)) { return; }
            if (!DA.GetData(4, ref font)) { return; }
            if (!DA.GetData(5, ref justification)) { return; }
            if (!DA.GetData(6, ref alignment)) { return; }


            //Options for justification
            TextJustification[] textJustification = new TextJustification[] {
                TextJustification.Left,
                TextJustification.Center,
                TextJustification.Right
            };

            //Options for alignment
            TextVerticalAlignment[] textVerticalAlignment = new TextVerticalAlignment[] {
                TextVerticalAlignment.Bottom,
                TextVerticalAlignment.Middle,
                TextVerticalAlignment.Top
            };


            //Create a headless doc
            var doc = RhinoDoc.CreateHeadless(null);

            //Create an custom dimstyle
            var id = doc.DimStyles.Add("Base");

            //Sets the base location for the plane
            plane.Origin = location;

            //Initalizes a doc font
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
                //If the custom font cant be found suplement it with a base font => Arial
                documentFont = doc.DimStyles[id].Font;
            }

            //Create and setup the text entity
            var te = new TextEntity();
            te.PlainText = text;
            te.Font = documentFont;
            te.Plane = plane;
            te.Justification = textJustification[justification];
            te.TextVerticalAlignment = textVerticalAlignment[alignment];

            //Get bbox for justification
            BoundingBox bbox = te.GetBoundingBox(true);

            //Adjust the plane to fit justification
            Line[] boxLine = bbox.GetEdges();
            Point3d boxCennter = bbox.Center;
            Line leftLine = boxLine[3];
            Point3d ajusterLoc = Point3d.Unset;
            Transform transform = Transform.Translation(leftLine.UnitTangent * -leftLine.Length / 2);
            ajusterLoc = boxLine[3].PointAt(boxLine[3].Length / 2);
            ajusterLoc.Transform(transform);
            plane.Origin = ajusterLoc;
            te.Plane = plane;

            //Plane to scale the font at
            Plane scalePlane = new Plane(plane);
            scalePlane.Origin = location;

            //Scale the font
            Transform scaleFactor = Transform.Scale(scalePlane, scale, scale, 0); // Replace with actual scale factorp

            //Convert text entity to curves
            Curve[] curves = te.CreateCurves(doc.DimStyles[id], false, 1, 0.1);

            // Scale all curves using the center of the bounding box
            foreach (Curve curve in curves)
            {
                curve.Transform(scaleFactor);
            }
            //Dispose the headless doc
            doc.Dispose();

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