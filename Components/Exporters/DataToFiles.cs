﻿using Grasshopper.Kernel;
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
            pManager.AddGenericParameter("Attributes", "A", "Attributes of the geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileNames", "F", "File names of the geometry", GH_ParamAccess.tree);
            pManager.AddTextParameter("FileEnding", "E", "File ending of the geometry", GH_ParamAccess.item, ".3dm");
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Files", "F", "Files", GH_ParamAccess.list);
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

            // Initialize ref vars
            GH_Structure<IGH_GeometricGoo> geometryTree = new GH_Structure<IGH_GeometricGoo>();
            GH_Structure<IGH_Goo> attributesTree = new GH_Structure<IGH_Goo>();
            GH_Structure<GH_String> fileNameTree = new GH_Structure<GH_String>();
            string fileEnding = string.Empty;

            // Retrieve the data from the input parameters
            if (!DA.GetDataTree(GEOMETRY_PARAM_INDEX, out geometryTree)) return; 
            if (!DA.GetDataTree(ATTRIBUTES_PARAM_INDEX, out attributesTree)) return; 
            if (!DA.GetDataTree(FILENAME_PARAM_INDEX, out fileNameTree)) return; 
            if (!DA.GetData(FILEENDING_PARAM_INDEX, ref fileEnding)) return; 

            // Convert trees to lists
            List<IGH_GeometricGoo> allGeo = geometryTree.AllData(true).OfType<IGH_GeometricGoo>().ToList();

            List<object> converterTree = attributesTree.AllData(true)
                .Select(goo => goo.ScriptVariable())
                .ToList();

            List<string> allStrings = fileNameTree.AllData(true)
                .Select(ghstr => (ghstr as GH_String)?.Value)
                .Where(s => s != null)
                .ToList();

            // Convert the generic objects to ObjectAttributes
            List<ObjectAttributes> allAttributes = converterTree.OfType<ObjectAttributes>().ToList();

            //List of json docs
            List<string> documents = new List<string>();

            //Loop though all geo items
            for (int i = 0; i < allGeo.Count; i++)
            {
                //Create a headless doc
                string docName = string.Empty;

                RhinoDoc doc = RhinoDoc.CreateHeadless(null);

                GeometryBase geo = allGeo[i].ScriptVariable() as GeometryBase;
                ObjectAttributes att;
                if (allAttributes.Count == 1)
                {
                    att = allAttributes[0];
                }
                else
                {
                    att = allAttributes[i];
                }
                if (geo == null || att == null)
                {
                    string msg = "Had problems finding geo or att";
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                }

                //Create current layer get name an color from attribute
                Layer layer = new Layer()
                {
                    Name = att.Name,
                    PlotColor = att.PlotColor,
                    Color = att.PlotColor
                };

                //Add layer to doc and get index to assign to object attribute
                int layerIndex = doc.Layers.Add(layer);

                //Set layer index to attribute
                att.LayerIndex = layerIndex;

                //Add geo to doc
                doc.Objects.Add(geo, att);


                string base64String =  Helpers.docToBase64(doc, ".3dm");

                FileData fileData = new FileData()
                {
                    fileName = att.Name,
                    data = base64String,
                    fileType = fileEnding
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

        class FileData
        {
            public string fileName { get; set; }
            public string data { get; set; }
            public string fileType { get; set; }
        }

    }





    //public class DataToFilesAttributes : GH_ComponentAttributes
    //{
    //    public override void CreateAttributes()
    //    {
    //        m_attributes = new DataToFilesAttributes(this);
    //    }
    //    public DataToFilesAttributes(DataToFiles owner) : base(owner) { }

    //    protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
    //    {
    //        switch (channel)
    //        {
    //            case GH_CanvasChannel.Wires:
    //                foreach (IGH_Param item in base.Owner.Params.Input)
    //                {
    //                    item.Attributes.RenderToCanvas(canvas, GH_CanvasChannel.Wires);
    //                }

    //                break;
    //            case GH_CanvasChannel.Objects:

    //                GH_Palette gH_Palette = GH_CapsuleRenderEngine.GetImpliedPalette(base.Owner);
    //                if (gH_Palette == GH_Palette.Normal && !base.Owner.IsPreviewCapable)
    //                {
    //                    gH_Palette = GH_Palette.Hidden;
    //                }
    //                bool left = base.Owner.Params.Input.Count == 0;
    //                bool right = true;

    //                GH_Capsule gH_Capsule = GH_Capsule.CreateCapsule(Bounds, gH_Palette);
    //                gH_Capsule.SetJaggedEdges(left, right);
    //                GH_PaletteStyle impliedStyle = GH_CapsuleRenderEngine.GetImpliedStyle(gH_Palette, Selected, base.Owner.Locked, base.Owner.Hidden);


    //                bool drawComponentBaseBox = true;
    //                bool drawParameterGrips = true;
    //                RectangleF MessageRectangle = RectangleF.Empty;
    //                bool drawComponentNameBox = true;
    //                bool drawParameterNames = true;
    //                bool drawZuiElements = true;




    //                if (drawParameterGrips)
    //                {
    //                    foreach (IGH_Param item in base.Owner.Params.Input)
    //                    {
    //                        gH_Capsule.AddInputGrip(item.Attributes.InputGrip.Y);
    //                    }
    //                }

    //                graphics.SmoothingMode = SmoothingMode.HighQuality;
    //                if (GH_Attributes<IGH_Component>.IsIconMode(base.Owner.IconDisplayMode))
    //                {
    //                    if (drawComponentBaseBox)
    //                    {
    //                        if (!string.IsNullOrWhiteSpace(base.Owner.Message))
    //                        {
    //                            MessageRectangle = gH_Capsule.RenderEngine.RenderMessage(graphics, base.Owner.Message, impliedStyle);
    //                        }

    //                        gH_Capsule.Render(graphics, impliedStyle);
    //                    }

    //                    if (drawComponentNameBox)
    //                    {
    //                        if (base.Owner.Icon_24x24 == null)
    //                        {
    //                            //gH_Capsule.RenderEngine.RenderIcon(graphics, Res_ObjectIcons.Icon_White_24x24, m_innerBounds);
    //                        }
    //                        else if (base.Owner.Locked)
    //                        {
    //                            gH_Capsule.RenderEngine.RenderIcon(graphics, base.Owner.Icon_24x24_Locked, m_innerBounds);
    //                        }
    //                        else
    //                        {
    //                            gH_Capsule.RenderEngine.RenderIcon(graphics, base.Owner.Icon_24x24, m_innerBounds);
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    if (drawComponentBaseBox)
    //                    {
    //                        if (base.Owner.Message != null)
    //                        {
    //                            gH_Capsule.RenderEngine.RenderMessage(graphics, base.Owner.Message, impliedStyle);
    //                        }

    //                        gH_Capsule.Render(graphics, impliedStyle);
    //                    }

    //                    if (drawComponentNameBox)
    //                    {
    //                        GH_Capsule gH_Capsule2 = GH_Capsule.CreateTextCapsule(m_innerBounds, m_innerBounds, GH_Palette.Black, base.Owner.NickName, GH_FontServer.LargeAdjusted, GH_Orientation.vertical_center, 3, 6);
    //                        gH_Capsule2.Render(graphics, Selected, base.Owner.Locked, hidden: false);
    //                        gH_Capsule2.Dispose();
    //                    }
    //                }

    //                if (drawComponentBaseBox)
    //                {
    //                    IGH_TaskCapableComponent iGH_TaskCapableComponent = base.Owner as IGH_TaskCapableComponent;
    //                    if (iGH_TaskCapableComponent != null)
    //                    {
    //                        if (iGH_TaskCapableComponent.UseTasks)
    //                        {
    //                            gH_Capsule.RenderEngine.RenderBoundaryDots(graphics, 2, impliedStyle);
    //                        }
    //                        else
    //                        {
    //                            gH_Capsule.RenderEngine.RenderBoundaryDots(graphics, 1, impliedStyle);
    //                        }
    //                    }
    //                }

    //                if (drawComponentNameBox && base.Owner.Obsolete && CentralSettings.CanvasObsoleteTags && canvas.DrawingMode == GH_CanvasMode.Control)
    //                {
    //                    GH_GraphicsUtil.RenderObjectOverlay(graphics, base.Owner, m_innerBounds);
    //                }

    //                if (drawParameterNames)
    //                {
    //                    RenderComponentParameters(canvas, graphics, base.Owner, impliedStyle);
    //                }

    //                if (drawZuiElements)
    //                {
    //                    RenderVariableParameterUI(canvas, graphics);
    //                }

    //                gH_Capsule.Dispose();
    //        break;
    //        }
    //    }
    //}
}

