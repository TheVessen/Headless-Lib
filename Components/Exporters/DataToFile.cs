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
using System.Collections;
using Grasshopper.Documentation;

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
            pManager.AddGeometryParameter("Geometry", "G", "Geometry to be exported", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Attributes", "A", "Attributes of the geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileName", "N", "Name of the File per List 1", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileEnding", "E", "File ending of the geometry", GH_ParamAccess.item, ".3dm");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("File", "F", "Files", GH_ParamAccess.list);
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

            // Initialize vars to store the data from DA
            GH_Structure<IGH_GeometricGoo> geometryList;
            GH_Structure<IGH_Goo> objectAttributesList;
            string fileEnding = string.Empty;
            GH_Structure<GH_String> fileName = new GH_Structure<GH_String>();

            // Retrieve the data from the input parameters
            if (!DA.GetDataTree(geometryParamIndex, out geometryList)) return;
            if (!DA.GetDataTree(attributesParamIndex, out objectAttributesList)) return;

            if (geometryList.PathCount != objectAttributesList.PathCount)
            {
                string msg = "The number of attributes & layer names have to be the same as geometries";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                return;
            }
            if (fileName.PathCount != objectAttributesList.PathCount)
            {
                string msg = "The number of filnames has to match the Trees";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                return;
            }

            if (!DA.GetDataTree(filenameParamIndex, out fileName)) return;
            if (!DA.GetData(fileendingParamIndex, ref fileEnding)) return;

            List<GH_String> fileLs = new List<GH_String>();

            for (int i = 0; i < geometryList.PathCount; i++)
            {
                //Create headless doc
                RhinoDoc doc = RhinoDoc.CreateHeadless(null);


                List<IGH_GeometricGoo> currentBranchGeo = geometryList.get_Branch(i).Cast<IGH_GeometricGoo>().ToList();
                List<IGH_Goo> currentBranchAttributes = objectAttributesList.get_Branch(i).Cast<IGH_Goo>().ToList();

                if (currentBranchAttributes.Count != 1 && currentBranchAttributes.Count != currentBranchGeo.Count)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The attribute count has to match the amount of geometries in a list or 1");
                    return;
                }

                for (int j = 0; j < currentBranchGeo.Count; j++)
                {
                    //Set geo and attributes
                    GeometryBase geo = currentBranchGeo[j].ScriptVariable() as GeometryBase;

                    ObjectAttributes atb;
                    if (currentBranchAttributes.Count == 1)
                    {
                        atb = currentBranchAttributes[0].ScriptVariable() as ObjectAttributes;
                    }
                    else
                    {
                        atb = currentBranchAttributes[j].ScriptVariable() as ObjectAttributes;
                    }

                    //Create current layer get name and color from attribute
                    if (atb != null)
                    {
                        Layer layer = new Layer()
                        {
                            Color = atb.ObjectColor,
                            PlotColor = atb.PlotColor,
                            Name = atb.Name
                        };

                        //Get layer index to assign layer to attribute
                        int layerIndex = doc.Layers.Add(layer);

                        //Set layer index to attribute
                        atb.LayerIndex = layerIndex;
                    }

                    //Add geo to doc
                    doc.Objects.Add(geo, atb);

                    //Convert doc to string
                }
                string base64String = Helpers.docToBase64(doc, fileEnding);

                //Add additional data to the file for serialization
                FileData fileData = new FileData() { FileName = fileName.get_Branch(i)[0] as string, Data = base64String, FileType = fileEnding };

                string b64File = JsonConvert.SerializeObject(fileData);

                fileLs.Add(new GH_String(b64File));

                //Free recources
                doc.Dispose();

                //
                //OUTPUT
                //
            }
            DA.SetData(0, fileLs);
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
            public string FileName { get; set; }
            public string Data { get; set; }
            public string FileType { get; set; }
        }

    }
}