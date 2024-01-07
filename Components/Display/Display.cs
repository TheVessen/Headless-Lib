using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Linq;
using System.Drawing;
using Headless.Components.Exporters;
using Headless.Utilities;
using Rhino.DocObjects;
using System.Net.Configuration;

namespace Headless.Components.Display
{
    public class WebDisplay : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Display class.
        /// </summary>
        public WebDisplay()
          : base("Display", "D",
              "Converts mesh to display file. To have the best control over the output mesh, convert the geometry first to mesh, otherwise the geo will be converted with some default params",
              "Headless", "Output")
        { }

        public class ThreeDisplay
        {
            public Color material { get; set; }
            public ThreeJsMeshData meshData { get; set; }
            public int id { get; set; }

        }
        
        public class ThreeJsMeshData
        {
            public List<double> Vertices { get; set; }
            public List<int> Faces { get; set; }
        }

        /// <summary>
        /// Comment out if you need the output params
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new NoOutputComponent<WebDisplay>(this);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geo", "G", "Geo to display", GH_ParamAccess.tree);
            pManager.AddColourParameter("Color", "C", "Color to display", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Display", "D", "Display", GH_ParamAccess.list);
            pManager.AddMeshParameter("DisplayMeshPreview", "P", "Preview Mesh", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            //ref vars
            GH_Structure<IGH_Goo> GHBaseGeo = new GH_Structure<IGH_Goo>();
            GH_Structure<GH_Colour> color = new GH_Structure<GH_Colour>();

            //Ref to vars
            if (!DA.GetDataTree(0, out GHBaseGeo)) return;
            if (!DA.GetDataTree(1, out color)) return;

            //Base params
            MeshingParameters mParams = new MeshingParameters();
            mParams.SimplePlanes = true;
            //vars
            List<Mesh> meshes = new List<Mesh>();
            List<GeometryBase> geometries = new List<GeometryBase>();

            foreach (IGH_Goo goo in GHBaseGeo.AllData(true))
            {
                try
                {
                    object internalData = goo.ScriptVariable();  // This method gets the underlying geometry data.

                    switch (internalData)
                    {
                        case Mesh mesh:
                            meshes.Add(mesh);
                            break;

                        case Brep brep:
                            Mesh[] brepMeshes = Mesh.CreateFromBrep(brep, mParams);
                            meshes.AddRange(brepMeshes);
                            break;

                        case Surface surface:
                            Mesh surfaceMesh = Mesh.CreateFromSurface(surface, mParams);
                            meshes.Add(surfaceMesh);
                            break;

                        default:
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not convert geo to mesh");
                            break;
                    }
                }
                catch
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Something went wrong in the casting process");
                }
            }

            if (meshes.Count == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No meshes to convert");
            }
            
            
            List<Color> allColors = color.AllData(true)
            .OfType<GH_Colour>()  // Ensure the item is of type GH_Colour.
            .Select(ghColour => ghColour.Value)  // Access the Value property.
            .ToList();

            int counter = 0;
            List<string> threeDisplays = new List<string>();
            List<GH_Mesh> previewMeshList = new List<GH_Mesh>();

            //convert to string and create preview
            foreach (Mesh mesh in meshes)
            {
                
                ThreeDisplay threeDisplay = new ThreeDisplay();
                var vertices = new List<double>();
                var faces = new List<int>();
                
                Mesh previewMesh = mesh.DuplicateMesh();
                
                //Create a preview for grasshopper
                if (allColors.Count == 1)
                {
                    threeDisplay.material = allColors[0];
                    previewMesh.VertexColors.CreateMonotoneMesh(allColors[0]);
                    GH_Mesh previewMeshWrapper = new GH_Mesh(previewMesh);
                    previewMeshList.Add(previewMeshWrapper);
                }
                else if (allColors.Count == meshes.Count)
                {
                    threeDisplay.material = allColors[counter];
                    previewMesh.VertexColors.CreateMonotoneMesh(allColors[counter]);
                    GH_Mesh previewMeshWrapper = new GH_Mesh(previewMesh);
                    previewMeshList.Add(previewMeshWrapper);
                }
                else
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Number of colors must be 1 or equal to number of meshes");
                }
                threeDisplay.id = counter;
                
                //Create the mesh data
                foreach (var vertex in mesh.Vertices)
                {
                    vertices.Add(vertex.X);
                    vertices.Add(vertex.Y);
                    vertices.Add(vertex.Z);
                }

                foreach (var face in mesh.Faces)
                {
                    if (face.IsQuad)
                    {
                        // First triangle
                        faces.Add(face.A);
                        faces.Add(face.B);
                        faces.Add(face.C);

                        // Second triangle
                        faces.Add(face.C);
                        faces.Add(face.D);
                        faces.Add(face.A);
                    }
                    else if (face.IsTriangle)
                    {
                        faces.Add(face.A);
                        faces.Add(face.B);
                        faces.Add(face.C);
                    }
                }
                
                ThreeJsMeshData threeJsMeshData = new ThreeJsMeshData();
                threeJsMeshData.Vertices = vertices;
                threeJsMeshData.Faces = faces;
                threeDisplay.meshData = threeJsMeshData;

                string obj = Newtonsoft.Json.JsonConvert.SerializeObject(threeDisplay);
                threeDisplays.Add(obj);
                counter++;
            }

            //
            //Output
            //

            DA.SetDataList(0, threeDisplays);
            DA.SetDataList(1, previewMeshList);
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
            get { return new Guid("3B108239-0103-4D4B-8407-534A78811090"); }
        }
    }
}