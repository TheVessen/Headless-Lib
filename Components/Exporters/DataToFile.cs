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
              "Headless", "Output")
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
            pManager.AddTextParameter("FileName", "N", "Name of the File per List 1", GH_ParamAccess.item, "DefaultName");
            pManager.AddTextParameter("FileEnding", "E", "File ending of the geometry", GH_ParamAccess.item, ".3dm");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("File", "F", "Files", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Set index
            const int GEOMETRY_PARAM_INDEX = 0;
            const int ATTRIBUTES_PARAM_INDEX = 1;
            const int FILENAME_PARAM_INDEX = 2;
            const int FILEENDING_PARAM_INDEX = 3;

            // Initialize vars to store the data from DA
            List<IGH_GeometricGoo> geometryList = new List<IGH_GeometricGoo>();
            List<IGH_Goo> objectAttributesList = new List<IGH_Goo>();
            string fileEnding = string.Empty;
            string fileName = string.Empty;

            // Retrieve the data from the input parameters
            if (!DA.GetDataList(GEOMETRY_PARAM_INDEX, geometryList)) return; 
            if (!DA.GetDataList(ATTRIBUTES_PARAM_INDEX, objectAttributesList)) return; 
            if (geometryList.Count != objectAttributesList.Count)
            {
                string msg = "The number of attributes & layernames have to be the same as geometries";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
            }

            if (!DA.GetData(FILENAME_PARAM_INDEX, ref fileName)) return; 
            if (!DA.GetData(FILEENDING_PARAM_INDEX, ref fileEnding)) return; 

            //Create headless doc
            RhinoDoc doc = RhinoDoc.CreateHeadless(null);
            for (int i = 0; i < geometryList.Count; i++)
            {
                //Set geo and attributes
                GeometryBase geo = geometryList[i].ScriptVariable() as GeometryBase;
                ObjectAttributes atb = objectAttributesList[i].ScriptVariable() as ObjectAttributes;

                //Create current layer get name an color from attribute
                Layer layer = new Layer() { 
                    Color=atb.ObjectColor,
                    PlotColor=atb.PlotColor,
                    Name=atb.Name
                };

                //Get layer index to assign layer to attribute
                int layerIndex = doc.Layers.Add(layer);

                //Set layer index to attribute
                atb.LayerIndex = layerIndex;

                //Add geo to doc
                doc.Objects.Add(geo, atb);

            }

            //Convert doc to string
            string base64String = Helpers.docToBase64(doc, fileEnding);

            //Add additional data to the file for serialization
            FileData fileData = new FileData() { fileName=fileName, data=base64String, fileType=fileEnding};

            string b64File = JsonConvert.SerializeObject(fileData);

            //Free recources
            doc.Dispose();

            //
            //OUTPUT
            //
            DA.SetData(0, b64File);
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