using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Linq;
using System.Drawing;

namespace Headless.Components.Display
{
    public class WebDisplay : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Display class.
        /// </summary>
        public WebDisplay()
          : base("Display", "D",
              "Converts mesh to display file",
              "Headless", "Display")
        { }

        public class ThreeDisplay
        {
            public Mesh geometry { get; set; }
            public Color material { get; set; }
            public int id { get; set; }

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to display", GH_ParamAccess.tree);
            pManager.AddColourParameter("Color", "C", "Color to display", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Display", "D", "Display", GH_ParamAccess.list);
            pManager.HideParameter(0);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Mesh> mesh = new GH_Structure<GH_Mesh>();
            GH_Structure<GH_Colour> color = new GH_Structure<GH_Colour>();

            if (!DA.GetDataTree(0, out mesh)) return;
            if (!DA.GetDataTree(1, out color)) return;

            List<Mesh> allMeshes = mesh.AllData(true)
                .OfType<GH_Mesh>()  // Ensure that the item is of type GH_Mesh.
                .Select(ghMesh => ghMesh.Value)  // Now you can access the Value property.
                .ToList();

            List<Color> allColors = color.AllData(true)
                .OfType<GH_Colour>()  // Ensure the item is of type GH_Colour.
                .Select(ghColour => ghColour.Value)  // Access the Value property.
                .ToList();

            int counter = 0;
            List<string> threeDisplays = new List<string>();
            foreach (Mesh m in allMeshes)
            {
                ThreeDisplay threeDisplay = new ThreeDisplay();
                threeDisplay.geometry = m;
                if (allColors.Count == 1)
                {
                    threeDisplay.material = allColors[0];
                }
                else if (allColors.Count == allMeshes.Count)
                {
                    threeDisplay.material = allColors[counter];
                }
                else
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of colors must be 1 or equal to number of meshes");
                }
                threeDisplay.id = counter;
                string obj = Newtonsoft.Json.JsonConvert.SerializeObject(threeDisplay);
                threeDisplays.Add(obj);
                counter++;
            }

            //Output that doesnt show in the gh ui component
            DA.SetDataList(0, threeDisplays);

        }
        protected override void AfterSolveInstance()
        {
            //Unregister the output parameter
            Params.UnregisterOutputParameter(Params.Output[0]);
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
            get { return new Guid("3B108239-0103-4D4B-8407-534A78811090"); }
        }
    }
}