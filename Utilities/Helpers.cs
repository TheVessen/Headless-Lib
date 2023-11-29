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
using ClosedXML.Excel;
using System.ComponentModel.Composition.Primitives;
using System.Security.RightsManagement;

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

        public static string docToRhinoFile(RhinoDoc doc, string fileEnding)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + fileEnding);

            try
            {

                doc.SaveAs(tempPath, 7);

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

        public static string csvToBase64(string csvString)
        {
            // Create a new workbook
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            // Split the input CSV string by double quotes ("\"\"") to separate sections
            string[] sections = csvString.Split(new string[] { "\"\"" }, StringSplitOptions.RemoveEmptyEntries);

            // Initialize row index
            int rowIndex = 1;

            // Process each section
            foreach (string section in sections)
            {
                // Split the section by line breaks to get rows
                string[] lines = section.Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                // Process each line
                foreach (string line in lines)
                {
                    // Split the line by commas to get columns
                    string[] columns = line.Split(',');

                    // Set the values in the worksheet
                    for (int col = 0; col < columns.Length; col++)
                    {
                        worksheet.Cell(rowIndex, col + 1).Value = columns[col];
                    }

                    rowIndex++;
                }
            }
            // Save the workbook to a MemoryStream
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);

                // Convert the MemoryStream to a byte array
                byte[] xlsxData = memoryStream.ToArray();

                // Convert the XLSX data to a Base64 encoded string
                string base64Data = Convert.ToBase64String(xlsxData);

                return base64Data;
                
            }
            // try
            // {
            //
            // }
            // catch (Exception ex)
            // {
            //     // Log or rethrow the exception as appropriate
            //     throw new Exception("An error occurred while converting CSV to Base64", ex);
            // }
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