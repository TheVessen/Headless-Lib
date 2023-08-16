using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using System.IO;
using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper;
using System.Drawing.Drawing2D;
using Grasshopper.Kernel.Attributes;


namespace Headless.Utilities
{
    public class Helpers
    {

        public static string docToBase64(RhinoDoc doc, string fileEnding)
        {
            // Create temp path for the docs
            string tempPath = Path.GetTempFileName() + fileEnding;
            doc.Export(tempPath);

            string base64String;
            using (FileStream fileStream = new FileStream(tempPath, FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                base64String = Convert.ToBase64String(memoryStream.ToArray());
            }

            File.Delete(tempPath);

            return base64String;
        }
    }
}