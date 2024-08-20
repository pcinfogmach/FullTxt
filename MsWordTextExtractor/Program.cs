using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System;
using System.IO;
using System.Xml.Linq;

namespace MsWordTextExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Admin\Desktop\וידאו מספק דרך רבת.docx";
            string htmlPath = filePath + ".html";

            ConvertDocxToHtml(filePath, htmlPath);
            System.Diagnostics.Process.Start("explorer.exe", htmlPath);
        }

        static void ConvertDocxToHtml(string docxFilePath, string htmlFilePath)
        {
            // Open the DOCX file with read-write access
            using (WordprocessingDocument docx = WordprocessingDocument.Open(docxFilePath, true))  // Set to true for read-write
            {
                // Create an instance of HtmlConverter
                HtmlConverterSettings settings = new HtmlConverterSettings()
                {
                    PageTitle = "Converted Document"
                };

                XElement html = HtmlConverter.ConvertToHtml(docx, settings);

                // Save the resulting HTML to a file
                File.WriteAllText(htmlFilePath, html.ToString(SaveOptions.DisableFormatting));
            }

            Console.WriteLine("Conversion complete. HTML saved to " + htmlFilePath);
        }
    }
}
