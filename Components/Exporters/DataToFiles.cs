using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Headless.Lib;
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
            pManager.AddTextParameter("LayerNames", "L", "Names of the layers", GH_ParamAccess.tree);
            pManager.AddColourParameter("LayerColors", "C", "Colors of the layers", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileNames", "F", "File names of the geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileEnding", "E", "File ending of the geometry", GH_ParamAccess.item, ".3dm"); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Base64", "B64", "Files", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            //Set index
            const int geometryParamIndex = 0;
            const int layerNamesParamIndex = 1;
            const int layerColorsParamIndex = 2;
            const int filenameParamIndex = 3;
            const int fileendingParamIndex = 4;

            // Initialize ref vars
            GH_Structure<IGH_GeometricGoo> geometryTree;
            GH_Structure<GH_String> layerNamesTree;
            GH_Structure<GH_Colour> layerColorsTree;
            GH_Structure<GH_String> fileNameTree;
            string fileEnding = string.Empty;

            // Retrieve the data from the input parameters
            if (!DA.GetDataTree(geometryParamIndex, out geometryTree)) return; 
            if (!DA.GetDataTree(layerNamesParamIndex, out layerNamesTree)) return; 
            if (!DA.GetDataTree(layerColorsParamIndex, out layerColorsTree)) return; 
            if (!DA.GetDataTree(filenameParamIndex, out fileNameTree)) return; 
            if (!DA.GetData(fileendingParamIndex, ref fileEnding)) return; 
            
            
            
            List<Color> allLayerColors = layerColorsTree.AllData(true)
                .Select(ghcol => ghcol is GH_Colour colorItem ? colorItem.Value : Color.Black)
                .ToList();
            List<string> allFileNames = fileNameTree.AllData(true)
                .Select(ghstr => (ghstr as GH_String)?.Value)
                .Where(s => s != null)
                .ToList();
            List<IGH_GeometricGoo> allGeo = geometryTree.AllData(true).OfType<IGH_GeometricGoo>().ToList();
            List<string> allLayerNames = layerNamesTree.AllData(true)
                .Select(ghstr => (ghstr as GH_String)?.Value)
                .Where(s => s != null)
                .ToList();
            List<string> allStrings = fileNameTree.AllData(true)
                .Select(ghstr => (ghstr as GH_String)?.Value)
                .Where(s => s != null)
                .ToList();

            //List of json docs
            List<string> documents = new List<string>();
            
            //Loop though all geo items
            for (int i = 0; i < allGeo.Count; i++)
            {
                RhinoDoc doc = RhinoDoc.CreateHeadless(null);
                GeometryBase geo = allGeo[i].ScriptVariable() as GeometryBase;

                string layerName = allLayerNames.Count > i ? allLayerNames[i] : "Default";
                Color layerColor = allLayerColors.Count > i ? allLayerColors[i] : Color.Black;

                Layer layer = new Layer()
                {
                    Name = layerName,
                    Color = layerColor
                };

                int layerIndex = doc.Layers.Add(layer);

                ObjectAttributes att = new ObjectAttributes
                {
                    LayerIndex = layerIndex
                };

                if (geo == null)
                {
                    string msg = "Had problems finding geo";
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                    return;
                }

                doc.Objects.Add(geo, att);

                string base64String = string.Empty;

                if (fileEnding == ".3dm")
                {
                    base64String = Helpers.docToRhinoFile(doc, fileEnding);
                }
                else
                {

                    base64String =  Helpers.docToBase64(doc, fileEnding);
                }

                if (string.IsNullOrEmpty(base64String))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not export file");
                }

                FileData fileData = new FileData()
                {
                    FileName = att.Name,
                    Base64String = base64String,
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
        

    }
}

