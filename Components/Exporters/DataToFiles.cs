using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using Headless.Utilities;

namespace Headless.Components.Exporters
{
    public class DataToFiles : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DataToFiles()
          : base("DataToFiles", "DTFs",
              "Description",
              "Headless", "Output")
        {
        }

        /// <summary>
        /// Comment out if you need the output params
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new NoOutputComponent<DataToFiles>(this);
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
            //Set index
            const int geometryParamIndex = 0;
            const int attributesParamIndex = 1;
            const int filenameParamIndex = 2;
            const int fileendingParamIndex = 3;

            // Initialize ref vars
            GH_Structure<IGH_GeometricGoo> geometryTree;
            GH_Structure<IGH_Goo> attributesTree;
            GH_Structure<GH_String> fileNameTree;
            string fileEnding = string.Empty;

            // Retrieve the data from the input parameters
            if (!DA.GetDataTree(geometryParamIndex, out geometryTree)) return; 
            if (!DA.GetDataTree(attributesParamIndex, out attributesTree)) return; 
            if (!DA.GetDataTree(filenameParamIndex, out fileNameTree)) return; 
            if (!DA.GetData(fileendingParamIndex, ref fileEnding)) return; 

            // Convert trees to lists
            List<IGH_GeometricGoo> allGeo = geometryTree.AllData(true).OfType<IGH_GeometricGoo>().ToList();

            List<object> converterTree = attributesTree.AllData(true)
                .Select(goo => goo.ScriptVariable())
                .ToList();

            List<string> allStrings = fileNameTree.AllData(true)
                .Select(ghstr => (ghstr as GH_String)?.Value)
                .Where(s => s != null)
                .ToList();

            // Convert the generic objects to ObjectAttributes
            List<ObjectAttributes> allAttributes = converterTree.OfType<ObjectAttributes>().ToList();

            //List of json docs
            List<string> documents = new List<string>();

            if (allAttributes.Count != 1 && allAttributes.Count != allGeo.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The attribute count has to match the amount of geometries in a list or 1");
                return;
            }

            //Loop though all geo items
            for (int i = 0; i < allGeo.Count; i++)
            {
                //Create a headless doc
                string docName = string.Empty;

                RhinoDoc doc = RhinoDoc.CreateHeadless(null);

                GeometryBase geo = allGeo[i].ScriptVariable() as GeometryBase;
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
                    return;
                }

                //Create current layer get name an color from attribute
                Layer layer = new Layer()
                {
                    Name = att.Name,
                    PlotColor = att.PlotColor,
                    Color = att.PlotColor
                };

                //Add layer to doc and get index to assign to object attribute
                int layerIndex = doc.Layers.Add(layer);

                //Set layer index to attribute
                att.LayerIndex = layerIndex;

                //Add geo to doc
                doc.Objects.Add(geo, att);


                string base64String =  Helpers.docToBase64(doc, ".3dm");

                FileData fileData = new FileData()
                {
                    FileName = att.Name,
                    Data = base64String,
                    FileType = fileEnding
                };

                //Convert class to JSON
                string serialFiles = JsonConvert.SerializeObject(fileData);

                //Add to list
                documents.Add(serialFiles);

                //Free resources
                doc.Dispose();
            }

            //
            //Output
            //
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
            public string FileName { get; set; }
            public string Data { get; set; }
            public string FileType { get; set; }
        }

    }
}

