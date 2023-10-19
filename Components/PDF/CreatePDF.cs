using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Office2010.Excel;
using SkiaSharp;
using QuestPDF;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rhino.Geometry;

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
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Page", "P", "Page", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Bits", "B", "N", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = System.IO.Path.Combine(desktopPath, "output.pdf");

            List<GH_ObjectWrapper> wPath = new List<GH_ObjectWrapper>();

            if (!DA.GetDataList(0, wPath)) return; // Use return here, or handle the error appropriately.

            var pages = wPath.Select(v => (v.Value as Document)).ToList();

            var pdfData = Document.Merge(pages).GeneratePdf();

            string base64String = Convert.ToBase64String(pdfData);
            DA.SetData(0, base64String);
            File.WriteAllBytes(outputPath, pdfData);
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