using org.apache.sis.@internal.jaxb.gmx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WordInterop = Microsoft.Office.Interop.Word;

namespace FullText.Helpers
{
    public static class WordToHtmlConverter
    {
        public static string Convert(string filePath)
        {
            string tempHtmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + "_FullTextExtractorTemp.html");
            
            WordInterop.Application wordApp = null;
            bool newApp = false;
            try
            {
                try
                {
                    wordApp = (WordInterop.Application)Marshal.GetActiveObject("Word.Application");
                }
                catch (COMException)
                {
                    wordApp = new WordInterop.Application();
                    newApp = true;
                }

                WordInterop.Document wordDoc = wordApp.Documents.Open(filePath, Visible: false);  
                wordDoc.SaveAs2(tempHtmlPath, WordInterop.WdSaveFormat.wdFormatFilteredHTML);
                wordDoc.Close(false);
            }
            catch 
            {
                File.WriteAllText(tempHtmlPath, TextExtractor.TikaTextExtractor(filePath));
            }
            finally
            {
                if (newApp && wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }

            if (File.Exists(tempHtmlPath)) { return tempHtmlPath; }
            else { return filePath; }           
        }
    }
}
