using PdfiumViewer.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordInterop = Microsoft.Office.Interop.Word;

namespace FullText.Helpers
{
    public class TextExtractor: IDisposable
    {
        string[] MsWordExtensions = { ".doc", ".docm", ".docx", ".dotx", ".dotm", ".dot", ".odt", ".rtf" };
        WordInterop.Application wordApp;

        public TextExtractor() 
        {
            //wordApp = Process.GetProcessesByName("");
            try { wordApp = new WordInterop.Application(); } catch { }      
        }

        public void Dispose()
        {
            try { wordApp.Quit(); } catch { }
        }

        public string ReadText(string filePath)
        {
            string content = string.Empty;
            string extension = Path.GetExtension(filePath);
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) {  content = PdfiumExtractor(filePath); }
            else if (MsWordExtensions.Contains(extension)) { }
            else { content = TikaTextExtractor(filePath); }
            return content;
        }

        string MsWordExtractor(string filePath)
        {
            string content = string.Empty;
            string tempHtmlPath = Path.Combine(Path.GetTempPath(), filePath + "FullTextExtractor.html");
            try
            {
                WordInterop.Document wordDoc = wordApp.Documents.Open(filePath);
                wordDoc.SaveAs2(tempHtmlPath, WordInterop.WdSaveFormat.wdFormatFilteredHTML);
                wordDoc.Close(false);
                content = File.ReadAllText(tempHtmlPath);
            }
            catch
            {
                content = TikaTextExtractor(filePath);
            }
            return content;
        }
  
        string PdfiumExtractor(string filePath)
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

        string TikaTextExtractor(string filePath)
        {
            try { return new TikaOnDotNet.TextExtraction.TextExtractor().Extract(filePath).Text; }
            catch { return string.Empty; }
           
        }
    }
}
