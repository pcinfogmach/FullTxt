using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using WordInterop = Microsoft.Office.Interop.Word;

namespace FullText.Helpers
{
    public static class HtmlConverter
    {
        public static string Convert(string filePath)
        {
            string tempHtmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + "_FullTextExtractorTemp.html");
            
            WordInterop.Application wordApp = null;
            bool newApp = false;

            try
            {
                Application.Current.Dispatcher.Invoke(() => {
                    try
                    {

                        wordApp = (WordInterop.Application)Marshal.GetActiveObject("Word.Application");
                        Application.Current.Exit += (s, e) => { try { Marshal.ReleaseComObject(wordApp); } catch { } };
                    }
                    catch (COMException)
                    {
                        wordApp = new WordInterop.Application();
                        newApp = true;
                        Application.Current.Exit += (s, e) => { try { wordApp.Quit(); Marshal.ReleaseComObject(wordApp); } catch { } };
                    }
                });
                //catch (System.Exception ex){ System.Windows.MessageBox.Show(ex.Message);}

                WordInterop.Document wordDoc = wordApp.Documents.Open(filePath, Visible: false);
                wordDoc.SaveAs2(tempHtmlPath, WordInterop.WdSaveFormat.wdFormatFilteredHTML);
                wordDoc.Close(false);
            }
            catch 
            {
                // Fallback to the alternative extraction method
                string content = TextExtractor.TikaTextExtractor(filePath);
                File.WriteAllText(tempHtmlPath, content);
            }
            finally
            {
                if (newApp && wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }

            if (File.Exists(tempHtmlPath))
            {
                return tempHtmlPath;
            }
            else
            {
                return filePath;
            }
        }

        public static string TikaConverter(string filePath)
        {
            string tempHtmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + "_FullTextExtractorTemp.html");

            string content = TextExtractor.TikaTextExtractor(filePath);
            File.WriteAllText(tempHtmlPath, content);

            if (File.Exists(tempHtmlPath))
            {
                return tempHtmlPath;
            }
            else
            {
                return filePath;
            }
        }
    }
}
