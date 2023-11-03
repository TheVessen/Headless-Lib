using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Headless.Components.Exporters;
using Headless.Lib;
using Headless.Utilities;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Rhino.UI.Controls;

namespace Headless.Components.PDF
{
    public class CreatePDF : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatePDF class.
        /// </summary>
        public CreatePDF()
            : base("CreatePDF", "CPDF",
                "Description",
                "Headless", "PDF")
        {
        }

        /// <summary>
        /// Comment out if you need the output params
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new NoOutputComponent<CreatePDF>(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("PDF Name", "PN", "Name of the PDF that should be used to download later",
                GH_ParamAccess.item);
            pManager.AddGenericParameter("Pages", "P", "Page", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Create Preview", "CP", "Create a preview of the PDF document",
                GH_ParamAccess.item, false);
            pManager.AddTextParameter("Document path", "DP", "Path to the document", GH_ParamAccess.item);
            
            pManager[3].Optional = true;
        }
        public override GH_Exposure Exposure => GH_Exposure.primary;

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
            QuestPDF.Settings.License = LicenseType.Community;

            List<GH_ObjectWrapper> wPath = new List<GH_ObjectWrapper>();
            string fileName = string.Empty;
            bool createPreview = false;
            string outputPath = string.Empty;

            if (!DA.GetData(0, ref fileName)) return;
            if (!DA.GetDataList(1, wPath)) return;
            if (!DA.GetData(2, ref createPreview)) return;
            DA.GetData(3, ref outputPath);

            var pages = wPath.Select(v => (v.Value as Document)).ToList();

            var pdfData = Document.Merge(pages).GeneratePdf();
            string base64String = Convert.ToBase64String(pdfData);
            
            string path = String.Empty;
            if (outputPath == string.Empty | outputPath == null)
            {
                outputPath = Path.GetTempPath();
            }
            path = Path.Combine(outputPath, fileName + ".pdf");

            //Create file object
            var fileData = new FileData()
            {
                FileName = fileName,
                Base64String = base64String,
                FileType = ".pdf"
            };

            string b64File = JsonConvert.SerializeObject(fileData);

            if (createPreview)
            {
                File.WriteAllBytes(path, pdfData);
            }
            
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
            get { return new Guid("5528597D-C1CC-4206-8811-3E79E3365A7D"); }
        }
    }
}