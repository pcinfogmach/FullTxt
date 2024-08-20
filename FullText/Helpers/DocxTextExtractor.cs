using DocumentFormat.OpenXml.Packaging;
using NPOI.HWPF;
using NPOI.HWPF.Extractor;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using WordInterop = Microsoft.Office.Interop.Word;

namespace MsWordTextExtractor
{
    public static class DocxTextExtractor
    {
        public static string Extract(string filePath)
        {
            try
            {
                return ReadAllTextParts(filePath);
            }
            catch
            {
                try
                {
                    return NpoiDocExtractor(filePath);
                }
                catch
                {
                    try
                    {
                        return WordInteropExtractor(filePath);
                    }
                    catch
                    {
                        try { return new TikaOnDotNet.TextExtraction.TextExtractor().Extract(filePath).Text; }
                        catch { return string.Empty; }
                    }
                }              
            }
        }

        static string ReadAllTextParts(string filePath)
        {
            try
            { 
            StringBuilder stb = new StringBuilder();
            using (WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Open(filePath, false))
            {
                var mainPart = wordprocessingDocument.MainDocumentPart;
                stb.AppendLine(ReadTextPart(mainPart.GetStream()));

                if (mainPart.FootnotesPart != null)
                {
                    string footNotes = ReadFootnotesPart(mainPart.FootnotesPart.GetStream());
                    if (!string.IsNullOrEmpty(footNotes))
                    {
                        stb.AppendLine();
                        stb.AppendLine(footNotes);
                    }
                }

                if (mainPart.EndnotesPart != null)
                {
                    string footNotes = ReadFootnotesPart(mainPart.FootnotesPart.GetStream());
                    if (!string.IsNullOrEmpty(footNotes))
                    {
                        stb.AppendLine();
                        stb.AppendLine(footNotes);
                    }
                }
            }
            return stb.ToString();
            }
            catch (FileFormatException ex)
            {
                Console.WriteLine($"File format exception: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        static string ReadTextPart(Stream partStream)
        {
            NameTable nameTable = new NameTable();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
            xmlNamespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            StringBuilder stringBuilder = new StringBuilder();

            XmlDocument xmlDocument = new XmlDocument(nameTable);
            xmlDocument.Load(partStream);

            XmlNodeList paragraphNodes = xmlDocument.SelectNodes("//w:p", xmlNamespaceManager);
            foreach (XmlNode paragraphNode in paragraphNodes)
            {
                ReadTextContent(stringBuilder, paragraphNode, xmlNamespaceManager);
                stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString().Trim();
        }

        static string ReadFootnotesPart(Stream partStream)
        {
            NameTable nameTable = new NameTable();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
            xmlNamespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            StringBuilder stringBuilder = new StringBuilder();

            XmlDocument xmlDocument = new XmlDocument(nameTable);
            xmlDocument.Load(partStream);

            XmlNodeList footnoteNodes = xmlDocument.SelectNodes("//w:footnote | .//w:endnote", xmlNamespaceManager);
            foreach (XmlNode footnoteNode in footnoteNodes)
            {
                string footnoteId = footnoteNode.Attributes["w:id"].Value;
                if (footnoteId == "-1" || footnoteId == "0") { continue; }
                stringBuilder.Append($"{footnoteId}");

                ReadTextContent(stringBuilder, footnoteNode, xmlNamespaceManager);

                stringBuilder.AppendLine();
            }
            return stringBuilder.ToString().Trim();
        }

        static void ReadTextContent(StringBuilder stringBuilder, XmlNode xmlNode, XmlNamespaceManager xmlNamespaceManager)
        {
            XmlNodeList textNodes = xmlNode.SelectNodes(".//w:t | .//w:tab | .//w:br | .//w:footnoteReference | .//w:numPr", xmlNamespaceManager);
            foreach (XmlNode textNode in textNodes)
            {
                switch (textNode.Name)
                {
                    case "w:t":
                        stringBuilder.Append(textNode.InnerText);
                        break;

                    case "w:tab":
                        stringBuilder.Append("\t");
                        break;

                    case "w:br":
                        stringBuilder.Append("\v");
                        break;

                    case "w:footnoteReference":
                        string footnoteId = textNode.Attributes["w:id"].Value;
                        stringBuilder.Append($"{footnoteId}");
                        break;

                    case "w:numPr":
                        XmlNode ilvlNode = textNode.SelectSingleNode(".//w:ilvl", xmlNamespaceManager);
                        XmlNode numIdNode = textNode.SelectSingleNode(".//w:numId", xmlNamespaceManager);
                        if (ilvlNode != null && numIdNode != null)
                        {
                            stringBuilder.Append("*");
                        }
                        break;
                }
            }
        }

        static string NpoiDocExtractor(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                HWPFDocument doc = new HWPFDocument(fileStream);
                WordExtractor extractor = new WordExtractor(doc);
                return extractor.Text;
            }
        }

        public static string WordInteropExtractor(string filePath)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + ".txt");


            try
            {
                using (WordApp wordApp = new WordApp())
                {
                    WordInterop.Document doc = null;
                    bool isFileAlreadyOpen = false;

                    foreach (WordInterop.Document openDoc in wordApp.App.Documents)
                    {
                        if (openDoc.FullName.Equals(Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase))
                        {
                            doc = openDoc;
                            isFileAlreadyOpen = true;
                            break;
                        }
                    }

                    if (doc == null) doc = wordApp.App.Documents.Open(filePath, ReadOnly: true, Visible: false);

                    var originalFormat = doc.SaveFormat;
                    doc.SaveAs2(tempFilePath, WordInterop.WdSaveFormat.wdFormatUnicodeText, Encoding: 65001, AddToRecentFiles: false);
                    if (isFileAlreadyOpen) doc.SaveAs2(filePath, originalFormat);

                    if (doc != null && !isFileAlreadyOpen) doc.Close(WordInterop.WdSaveOptions.wdDoNotSaveChanges);
                }

                return File.ReadAllText(tempFilePath);
            }
            finally
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
            }
        }
    }

    class WordApp : IDisposable
    {
        public Microsoft.Office.Interop.Word.Application App;
        bool isNewApp;

        public WordApp()
        {
            try
            {
                App = (WordInterop.Application)Marshal.GetActiveObject("Word.Application");
            }
            catch (COMException)
            {
                App = new WordInterop.Application();
                isNewApp = true;
            }
        }

        public void Dispose()
        {
            if (isNewApp && App != null)
            {
                App.Quit();
                Marshal.ReleaseComObject(App);
            }
        }
    }
}
