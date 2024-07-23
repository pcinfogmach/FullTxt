using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WordInterop = Microsoft.Office.Interop.Word;

namespace FullText.Helpers
{
    public static class TextExtractor
    {
        static string[] MsWordExtensions = { ".doc", ".docm", ".docx", ".dotx", ".dotm", ".dot", ".odt", ".rtf" };

        public static string ReadText(string filePath)
        {
            string content = string.Empty;
            string extension = Path.GetExtension(filePath);
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) {  content = PdfiumExtractor(filePath); }
            else if (MsWordExtensions.Contains(extension)) { content = MsWordExtractor(filePath); }
            else { content = TikaTextExtractor(filePath); }
            return content;
        }

        static string MsWordExtractor(string filePath)
        {
            string tempPath = WordToHtmlConverter.Convert(filePath);
            return TikaTextExtractor(tempPath);
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
