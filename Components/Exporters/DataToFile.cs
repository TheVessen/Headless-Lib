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
    public class DataToFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DataToFile class.
        /// </summary>
        public DataToFile()
          : base("DataToFile", "DTF",
              "Description",
              "Headless", "Exporter")
        {
        }

        /// <summary>
        /// Comment out if you need the output params
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new NoOutputComponent<DataToFile>(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to be exported", GH_ParamAccess.list);
            pManager.AddGenericParameter("Attributes", "A", "Attributes of the geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("LayerNames", "L", "Layer names", GH_ParamAccess.list);
            pManager.AddTextParameter("FileEnding", "E", "File ending of the geometry", GH_ParamAccess.item, ".3dm");
            pManager.AddTextParameter("FileName", "N", "Name of the File per List 1", GH_ParamAccess.item, "DefaultName");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Files", "F", "Files", GH_ParamAccess.item);
            pManager.HideParameter(0);
            pManager[0].Optional = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Initialize the data trees to hold the input data
            List<IGH_GeometricGoo> geometryList = new List<IGH_GeometricGoo>();
            List<IGH_Goo> attributesList = new List<IGH_Goo>();
            List<GH_String> layerNames = new List<GH_String>();
            string fileEnding = string.Empty;
            string fileName = string.Empty;


            // Retrieve the data from the input parameters
            if (!DA.GetDataList(0, geometryList)) return; // The 0 here refers to the first parameter index
            if (!DA.GetDataList(1, attributesList)) return; // The 1 here refers to the second parameter index
            if (!DA.GetDataList(2, layerNames)) return; // The 2 here refers to the third parameter index
            if (!DA.GetData(3, ref fileEnding)) return; // The 3 here refers to the fourth parameter index
            if (!DA.GetData(4, ref fileEnding)) return; // The 3 here refers to the fourth parameter index


            // This assumes that you want to convert the generic objects to ObjectAttributes
            List<ObjectAttributes> allAttributes = attributesList.OfType<ObjectAttributes>().ToList();

            List<string> documents = new List<string>();

            RhinoDoc doc = RhinoDoc.CreateHeadless(null);
            for (int i = 0; i < geometryList.Count; i++)
            {
                IGH_GeometricGoo geo = geometryList[i];
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
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
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




                doc.Dispose();
            }
            FileData fileData = new FileData();
            fileData.fileName = fileName;
            string base64String = Helpers.docToBase64(doc, ".3dm");
            fileData.data = base64String;
            fileData.fileType = fileEnding;
            string serialFile = JsonConvert.SerializeObject(fileData);

            //Set output
            DA.SetData(0, serialFile);
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
            get { return new Guid("A51C8F6A-D422-4387-8170-F9F34D8E5351"); }
        }

        class FileData
        {
            public string fileName { get; set; }
            public string data { get; set; }
            public string fileType { get; set; }
        }

    }
}