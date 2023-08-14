using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.DocObjects;
using Rhino.Geometry;

using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System.Linq;
using GH_IO.Types;
using System.ComponentModel;
using Rhino;
using Rhino.NodeInCode;
using Newtonsoft.Json;

namespace Headless.Components.Exporters
{
    public class DataToFiles : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DataToFiles()
          : base("DataToFiles", "DTF",
              "Description",
              "Headless", "Exporter")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to be exported", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Attributes", "A", "Attributes of the geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileNames", "F", "File names of the geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileEnding", "E", "File ending of the geometry", GH_ParamAccess.item, ".3dm");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Files", "F", "Files", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize the data trees to hold the input data
            GH_Structure<IGH_GeometricGoo> geometryTree = new GH_Structure<IGH_GeometricGoo>();
            GH_Structure<IGH_Goo> attributesTree = new GH_Structure<IGH_Goo>();
            GH_Structure<GH_String> fileNamesTree = new GH_Structure<GH_String>();
            string fileEnding = string.Empty;

            // Retrieve the data from the input parameters
            if (!DA.GetDataTree(0, out geometryTree)) return; // The 0 here refers to the first parameter index
            if (!DA.GetDataTree(1, out attributesTree)) return; // The 1 here refers to the second parameter index
            if (!DA.GetDataTree(2, out fileNamesTree)) return; // The 2 here refers to the third parameter index
            if (!DA.GetData(3, ref fileEnding)) return; // The 3 here refers to the fourth parameter index


            // Convert the data trees to lists
            List<IGH_GeometricGoo> allGeo = geometryTree.AllData(true).OfType<IGH_GeometricGoo>().ToList();


            List<object> converterTree = attributesTree.AllData(true)
                .Select(goo => goo.ScriptVariable())
                .ToList();

            List<string> allStrings = fileNamesTree.AllData(true)
                .Select(ghstr => (ghstr as GH_String)?.Value)
                .Where(s => s != null)
                .ToList();

            // This assumes that you want to convert the generic objects to ObjectAttributes
            List<ObjectAttributes> allAttributes = converterTree.OfType<ObjectAttributes>().ToList();

            List<string> documents = new List<string>();

            for (int i = 0; i < allGeo.Count; i++)
            {
                //Create a headless doc
                string docName = string.Empty;

                RhinoDoc doc = RhinoDoc.CreateHeadless(null);
                IGH_GeometricGoo geo = allGeo[i];
                ObjectAttributes att;
                if (allAttributes.Count == 1)
                {
                    att = allAttributes[0];
                }
                else
                {
                    att = allAttributes[i];
                }
                if (geo == null || att == null)
                {
                    string msg = "Had problems finding geo or att";
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                }

                //Create current layer get name an color from attribute
                Layer layer = new Layer();
                layer.Name = att.Name;
                layer.PlotColor = att.PlotColor;
                layer.Color = att.ObjectColor;
                int layerIndex = doc.Layers.Add(layer);

                //Set layer index to attribute
                att.LayerIndex = layerIndex;

                GeometryBase geoBase = geo.ScriptVariable() as GeometryBase;

                //Add geo to doc
                doc.Objects.Add(geoBase, att);

                string base64String = Utilities.docToBase64(doc, ".3dm");

                FileData fileData = new FileData();
                fileData.fileName = att.Name;
                fileData.data = base64String;
                fileData.fileType = fileEnding;

                string serialFiles = JsonConvert.SerializeObject(fileData);

                documents.Add(serialFiles);
                doc.Dispose();
            }

            //Set output
            DA.SetDataList(0, documents);
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
            get { return new Guid("8821F5CA-316E-44FA-B0D0-E3D8527952E3"); }
        }

        class FileData
        {
            public string fileName { get; set; }
            public string data { get; set; }
            public string fileType { get; set; }
        }

    }
}