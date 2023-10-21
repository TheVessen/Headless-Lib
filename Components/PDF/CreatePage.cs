﻿using System;
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
            pManager.AddGenericParameter("Paths", "CD", "CurveData", GH_ParamAccess.list);
            pManager.AddGenericParameter("TextBlob", "TB", "Text Blob", GH_ParamAccess.list);
            pManager.AddNumberParameter("Height", "H", "Page height", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Width", "W", "Page width", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("PageMargin", "PM", "Margin on the sides in mm", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("TitleSize", "TS", "Size for title", GH_ParamAccess.item, 30);
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
            double height = 1000;
            double widht = 1000;
            double margin = 10;
            double titleSize = 30;

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
            
            if (!DA.GetData(3, ref height)) return;
            if (!DA.GetData(4, ref widht)) return;
            if (!DA.GetData(5, ref margin)) return;
            if (!DA.GetData(6, ref titleSize)) return;
            
            
            // Convert data to the desired types
            var pathData = curveDataObjects.Select(v => v.Value as SkiaCurveData).ToList();
            var textBlobs = textBlobObjects.Select(v => v.Value as TextBlob).ToList();
            
            var doc = Document.Create(document =>
            {
                document.Page(pg =>
                {
                    pg.Size(Convert.ToSingle(widht),Convert.ToSingle(height), Unit.Millimetre);

                    pg.Header().Text(textTitle =>
                    {
                        textTitle.Span(pageTile)
                            .FontSize(Convert.ToSingle(titleSize)).FontColor(Colors.Black);
                    });

                    pg.Margin(Convert.ToSingle(margin), Unit.Millimetre);
                    
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
                        foreach (var tb in textBlobs)
                        {
                            
                            // Measure the width and height of the text

                            var newP = tb.TextPaint.GetTextPath(tb.Text, tb.Position.X, tb.Position.Y*-1);
                            
                            using (var transformedPath = new SKPath())
                            {
                                newP.Transform(combinedMatrix, transformedPath);
                                
                                SKRect pathBounds = transformedPath.Bounds;
                                float centerX = pathBounds.MidX;
                                
                                SKRect textBounds = new SKRect();
                                tb.TextPaint.MeasureText(tb.Text, ref textBounds);
                                float centerY = pathBounds.MidY + textBounds.Height / 2;
                                
                                canvas.DrawText(tb.Text, centerX, centerY, tb.TextPaint);
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