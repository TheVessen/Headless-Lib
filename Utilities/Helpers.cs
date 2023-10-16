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
using PdfSharp.Pdf.Content.Objects;


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

        public static string csvToBase64(string csvStr)
        {
            try
            {
                string[,] dataArray = ConvertCsvStringToDataArray(csvStr);

                string csvString = ConvertDataArrayToCsvString(dataArray);

                // Encode the CSV content to Base64
                string base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(csvString));
        
                return base64Data;
            }
            catch (Exception ex)
            {
                // Log or rethrow the exception as appropriate
                throw new Exception("An error occurred while converting CSV to Base64", ex);
            }
        }

        static string ConvertDataArrayToCsvString(string[,] dataArray)
        {
            int numRows = dataArray.GetLength(0);
            int numCols = dataArray.GetLength(1);

            // Create a StringBuilder to construct the CSV string
            System.Text.StringBuilder csvBuilder = new System.Text.StringBuilder();

            // Append the header row
            for (int col = 0; col < numCols; col++)
            {
                csvBuilder.Append(dataArray[0, col]);
                if (col < numCols - 1)
                {
                    csvBuilder.Append(",");
                }
            }
            csvBuilder.AppendLine(); // Add a newline after the header

            // Append the data rows
            for (int row = 1; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    csvBuilder.Append(dataArray[row, col]);
                    if (col < numCols - 1)
                    {
                        csvBuilder.Append(",");
                    }
                }
                csvBuilder.AppendLine(); // Add a newline after each row
            }

            return csvBuilder.ToString();
        }

        static string[,] ConvertCsvStringToDataArray(string csvString)
        {
            // Split the CSV string into lines
            string[] lines = csvString.Split('\n');

            // Determine the number of rows and columns
            int numRows = lines.Length;
            int numCols = lines[0].Split(',').Length;

            // Initialize the dataArray with the determined dimensions
            string[,] dataArray = new string[numRows, numCols];

            // Populate the dataArray with values from the CSV
            for (int row = 0; row < numRows; row++)
            {
                string[] values = lines[row].Split(',');
                for (int col = 0; col < numCols; col++)
                {
                    dataArray[row, col] = values[col];
                }
            }

            return dataArray;
        }
    }
    
}