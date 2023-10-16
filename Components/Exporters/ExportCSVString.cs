using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Headless.Utilities;
using Newtonsoft.Json;
using Rhino.Geometry;

namespace Headless.Components.Exporters
{
    public class ExportCSVString : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExportCSVString class.
        /// </summary>
        public ExportCSVString()
          : base("ExportCSVString", "ECSV",
              "Exports a csv string to base64",
              "Headless", "Output")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "D", "CSV String", GH_ParamAccess.item);
            pManager.AddTextParameter("FileName", "FN", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Base64", "B64", "Base 64 string output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;
            string fileName = string.Empty;
            DA.GetData(0, ref data);
            DA.GetData(1, ref fileName);

            var b64 = Helpers.csvToBase64(data);

            FileData fileData = new FileData()
            {
                FileName = fileName,
                Data = b64,
                FileType = ".xlsx"
            };
            
            string b64File = JsonConvert.SerializeObject(fileData);


            DA.SetData(0, b64File);
        }

        class FileData
        {
            public string FileName { get; set; }
            public string Data { get; set; }
            public string FileType { get; set; }
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
            get { return new Guid("843014D9-216E-45D0-8C08-4B82D45A0E58"); }
        }
    }

}