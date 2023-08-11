using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using System.IO;


public static class Utilities
{

    public static string docToBase64(RhinoDoc doc, string fileEnding)
    {
        string base64String = string.Empty;
        //Create temp path for the docs
        string tempPath = Path.GetTempFileName() + fileEnding;
        doc.Export(tempPath);

        //Open and write file to base64
        using (FileStream fileStream = new FileStream(tempPath, FileMode.OpenOrCreate))
        {
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);

            byte[] buffer = memoryStream.ToArray();
            base64String = Convert.ToBase64String(buffer);

            memoryStream.Close();
        }
        File.Delete(tempPath);
        return base64String;
    }
}