using PdfiumViewer.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace FullText.Helpers
{
    public static class TextExtractor
    {
        static string[] MsWordExtensions = { ".doc", ".docm", ".docx", ".dotx", ".dotm", ".dot", ".odt", ".rtf" };

        public static string ReadText(string filePath)
        {
            string content = string.Empty;
            try
            {
                string extension = Path.GetExtension(filePath);
                if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) { content = PdfiumExtractor(filePath); }
                else if (MsWordExtensions.Contains(extension)) { content = MsWordExtractor(filePath); }
                else { content = TikaTextExtractor(filePath); }
            }
            catch (Exception ex)
            {
                HebrewMessageBox.InformationMessageBox(ex.Message);
            }
            return content;          
        }

        static string MsWordExtractor(string filePath)
        {
            try
            {
                string tempPath = WordToHtmlConverter.Convert(filePath);
                return TikaTextExtractor(tempPath);
            }
            catch
            {
                return TikaTextExtractor(filePath);
            }
        }

        static string PdfiumExtractor(string filePath)
        {
            try
            {
                StringBuilder stb = new StringBuilder();
                var doc = PdfDocument.Load(filePath);
                for (int i = 0; i < doc.PageCount; i++)
                {
                    stb.Append(doc.GetPdfText(i));
                }
                return stb.ToString();
            }
            catch
            {
                return TikaTextExtractor(filePath);
            }
        }

       public static string TikaTextExtractor(string filePath)
        {
            try { return new TikaOnDotNet.TextExtraction.TextExtractor().Extract(filePath).Text; }
            catch { return string.Empty; }        
        }
    }
}
