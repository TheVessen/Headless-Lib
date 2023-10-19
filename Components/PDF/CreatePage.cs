using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Headless.Lib;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rhino.Geometry;
using SkiaSharp;

namespace Headless.Components.PDF
{
    public class CreatePage : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreatePage class.
        /// </summary>
        public CreatePage()
            : base("CreatePage", "Nickname",
                "Description",
                "Headless", "PDF")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("CurveData", "CD", "CurveData", GH_ParamAccess.list);
            pManager.AddTextParameter("Page Tile", "PT", "", GH_ParamAccess.item, "Title");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Page", "P", "QuestPDF", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pageTile = string.Empty;


            List<GH_ObjectWrapper> cData = new List<GH_ObjectWrapper>();

            if (!DA.GetDataList(0, cData)) return; // Use return here, or handle the error appropriately.
            if (!DA.GetData(1, ref pageTile)) return;

            var pathData = cData.Select(v => (v.Value as SkiaCurveData)).ToList();

            var doc = Document.Create(pdf =>
            {
                pdf.Page(page =>
                {
                    page.Size(PageSizes.A3.Landscape());

                    page.Header().Text(text =>
                    {
                        text.Span(pageTile)
                            .FontSize(30).FontColor(Colors.Black);
                    });
                    
                    page.Margin(2, Unit.Centimetre);

                    // Add paths
                    page.Content().Canvas((canvas, space) =>
                    {
                        // Calculate the combined bounding box of all paths.
                        SKRect combinedBounds = new SKRect();
                        foreach (var curveData in pathData)
                        {
                            combinedBounds = SKRect.Union(combinedBounds, curveData.Path.Bounds);
                        }
                        

                        // Calculate the scale factor to fit bounding box in the canvas.
                        float xScale = space.Width / combinedBounds.Width;
                        float yScale = space.Height / combinedBounds.Height;
                        float scale = Math.Min(xScale, yScale);

                        // Create a scaling matrix.
                        var scaleMatrix = SKMatrix.CreateScale(scale, scale);
                        
                        float scaledWidth = combinedBounds.Width * scale;
                        float scaledHeight = combinedBounds.Height * scale;

                        // Calculate the translation to center the scaled bounding box on the canvas.
                        float dx = (space.Width - scaledWidth) / 2f - combinedBounds.Left * scale;
                        float dy = (space.Height - scaledHeight) / 2f - combinedBounds.Top * scale;

                        // Create a translation matrix.
                        var translationMatrix = SKMatrix.CreateTranslation(dx, dy);
                        var combinedMatrix = SKMatrix.Concat(translationMatrix, scaleMatrix); // Concatenate matrices: first scale, then translate.

                        // Apply the combined transformation to each path and draw it.
                        foreach (var curveData in pathData)
                        {
                            using (var transformedPath = new SKPath())
                            {
                                curveData.Path.Transform(combinedMatrix, transformedPath);
                                canvas.DrawPath(transformedPath, curveData.Paint);
                            }
                        }
                    });
                });
            });


            DA.SetData(0, doc);
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
            get { return new Guid("2794E5CB-9960-42F1-8C4E-B39BF72603BA"); }
        }
    }
}