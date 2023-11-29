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
using Headless.Lib;

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
            pManager.AddTextParameter("FileName", "N", "Name of the File per List 1", GH_ParamAccess.item);
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
            // Initialize vars to store the data from DA
            GH_Structure<IGH_GeometricGoo> geometryList;
            GH_Structure<IGH_Goo> objectAttributesList;
            string fileName = string.Empty;
            string fileEnding = string.Empty;

            // Retrieve the data from input parameters
            if (!RetrieveData(DA, 0, out  geometryList)) return;
            if (!RetrieveData(DA, 1, out objectAttributesList)) return;
            if (!RetrieveData(DA, 2, out  fileName)) return;
            if (!RetrieveData(DA, 3, out fileEnding)) return;

            // Create headless doc
            RhinoDoc doc = RhinoDoc.CreateHeadless(null);


            // Check attribute count validity
            List<IGH_Goo> allAttributes = objectAttributesList.AllData(true).ToList();
            if (allAttributes.Count != 1 && allAttributes.Count != geometryList.PathCount)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The attribute count has to match the amount of geometries in a list or be 1");
                return;
            }

            // Iterate over geometries
            foreach (var path in geometryList.Paths)
            {
                List<IGH_GeometricGoo> currentBranchGeo = geometryList.get_Branch(path).Cast<IGH_GeometricGoo>().ToList();
                List<IGH_Goo> currentBranchAttributes = objectAttributesList.get_Branch(path).Cast<IGH_Goo>().ToList();

                ObjectAttributes atb = (currentBranchAttributes.Count == 1) ? currentBranchAttributes[0].ScriptVariable() as ObjectAttributes : null;

                foreach (var geoGoo in currentBranchGeo)
                {
                    GeometryBase geo = geoGoo.ScriptVariable() as GeometryBase;

                    if (atb == null && allAttributes.Count == 1)
                    {
                        atb = allAttributes[0].ScriptVariable() as ObjectAttributes;
                    }

                    if (atb != null)
                    {
                        Layer layer = new Layer()
                        {
                            Color = atb.ObjectColor,
                            PlotColor = atb.PlotColor,
                            Name = atb.Name
                        };

                        int layerIndex = doc.Layers.Add(layer);
                        atb.LayerIndex = layerIndex;
                    }

                    doc.Objects.Add(geo, atb);
                }
            }

            string base64String = string.Empty;

            if (fileEnding == ".3dm")
            {
                base64String = Helpers.docToRhinoFile(doc, fileEnding);
            }
            else
            {

                base64String = Helpers.docToBase64(doc, fileEnding);
            }


            if (string.IsNullOrEmpty(base64String))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not export file");
            }

            FileData fileData = new FileData() { FileName = fileName, Base64String = base64String, FileType = fileEnding };
            string b64File = JsonConvert.SerializeObject(fileData);
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
        

    // Overload for GH_Structure data
    private bool RetrieveData<T>(IGH_DataAccess DA, int index, out GH_Structure<T> data) where T : class, IGH_Goo
    {
        data = null;
        if (!DA.GetDataTree(index, out GH_Structure<T> dataTree))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to get data at index {index}");
            return false;
        }
        data = dataTree;
        return true;
    }

    // Overload for simple data types like string
    private bool RetrieveData(IGH_DataAccess DA, int index, out string data)
    {
        data = string.Empty;
        if (!DA.GetData(index, ref data))
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Failed to get data at index {index}");
            return false;
        }
        return true;
    }
    }

}