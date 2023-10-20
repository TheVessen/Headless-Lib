using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Headless.Lib;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
            pManager.AddTextParameter("Page Tile", "PT", "", GH_ParamAccess.item, "Title");
            pManager.AddGenericParameter("CurveData", "CD", "CurveData", GH_ParamAccess.list);
            pManager.AddGenericParameter("TextBlob", "TB", "Text Blob", GH_ParamAccess.list);
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
            QuestPDF.Settings.License = LicenseType.Community;

            string pageTile = string.Empty;

            List<GH_ObjectWrapper> curveDataObjects = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> textBlobObjects = new List<GH_ObjectWrapper>();

            if (!DA.GetData(0, ref pageTile)) return;

            if (!DA.GetDataList(1, curveDataObjects))
            {
                // Handle error or provide feedback to the user.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to retrieve curve data.");
                return;
            }

            if (!DA.GetDataList(2, textBlobObjects))
            {
                // Handle error or provide feedback to the user.
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to retrieve text blobs.");
                return;
            }

            // Convert data to the desired types
            var pathData = curveDataObjects.Select(v => v.Value as SkiaCurveData).ToList();
            var textBlobs = textBlobObjects.Select(v => v.Value as TextBlob).ToList();
            
            var doc = Document.Create(document =>
            {
                document.Page(pg =>
                {
                    pg.Size(2000,2000);

                    pg.Header().Text(textTitle =>
                    {
                        textTitle.Span(pageTile)
                            .FontSize(30).FontColor(Colors.Black);
                    });

                    pg.Margin(1, Unit.Centimetre);

                    pg.Content().Canvas((canvas, space) =>
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
                        var combinedMatrix =
                            SKMatrix.Concat(translationMatrix,
                                scaleMatrix); // Concatenate matrices: first scale, then translate.
                        // Apply the combined transformation to each path and draw it.
                        foreach (var curveData in pathData)
                        {
                            using (var transformedPath = new SKPath())
                            {
                                curveData.Path.Transform(combinedMatrix, transformedPath);
                                canvas.DrawPath(transformedPath, curveData.Paint);
                            }
                        }
                        var origin = new SKPoint() { X = space.Width / 2, Y = space.Height / 2 };

                        foreach (var tb in textBlobs)
                        {
                            // Create the path for the original position
                            var newPath = tb.TextPaint.GetTextPath(tb.Text, tb.Position.X, tb.Position.Y);
    
                            // Get bounds of the path
                            SKRect pathBounds = newPath.Bounds;
    
                            // Calculate how much you need to adjust the position to center the text path
                            float dxx = tb.Position.X - (pathBounds.Width / 2 + pathBounds.Left);
                            float dyy = tb.Position.Y - (pathBounds.Height / 2 + pathBounds.Top);

                            // Create a translation matrix for the adjustment
                            var adjustmentMatrix = SKMatrix.CreateTranslation(dxx, dyy);

                            // Apply the adjustment to the path
                            newPath.Transform(adjustmentMatrix);
    
                            using (var transformedPath = new SKPath())
                            {
                                newPath.Transform(combinedMatrix, transformedPath);
                                canvas.DrawPath(transformedPath, tb.TextPaint);
                            }
                        }
                    });
                });
            });

            var d = doc.GeneratePdf();

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