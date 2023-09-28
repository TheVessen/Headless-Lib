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
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + fileEnding);

            try
            {
                doc.Export(tempPath);

                if (!File.Exists(tempPath))
                    throw new Exception("Failed to export the document.");

                string base64String;
                using (FileStream fileStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    base64String = Convert.ToBase64String(memoryStream.ToArray());
                }

                return base64String;
            }
            catch (Exception ex)
            {
                // Log or rethrow the exception as appropriate
                throw new Exception("An error occurred while converting doc to Base64", ex);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}