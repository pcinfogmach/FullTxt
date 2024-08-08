using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MsWordTextExtractor
{
    public static class XMlPowerToolsExtractor
    {
        public static string Extract(string filePath)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                StringBuilder extractedText = new StringBuilder();

                // Extract main document text
                var docText = ConvertToXElement(wordDoc.MainDocumentPart.Document.Body);
                extractedText.Append(docText.Value);

                // Extract footnotes text
                if (wordDoc.MainDocumentPart.FootnotesPart != null)
                {
                    var footnotes = wordDoc.MainDocumentPart.FootnotesPart.Footnotes;
                    foreach (var footnote in footnotes.Elements())
                    {
                        extractedText.AppendLine();
                        extractedText.AppendLine(ConvertToXElement(footnote).Value);
                    }
                }

                // Extract endnotes text
                if (wordDoc.MainDocumentPart.EndnotesPart != null)
                {
                    var endnotes = wordDoc.MainDocumentPart.EndnotesPart.Endnotes;
                    foreach (var endnote in endnotes.Elements())
                    {
                        extractedText.AppendLine();
                        extractedText.AppendLine(ConvertToXElement(endnote).Value);
                    }
                }

                return extractedText.ToString();
            }
        }

        static XElement ConvertToXElement(OpenXmlElement element)
        {
            return XElement.Parse(element.OuterXml);
        }

        public static void ConvertDocxToHtml(string sourceFile, string outputFile)
        {
            // Open the document in read-write mode
            using (WordprocessingDocument wDoc = WordprocessingDocument.Open(sourceFile, true))
            {
                HtmlConverterSettings settings = new HtmlConverterSettings()
                {
                    PageTitle = "Converted HTML",
                };

                XElement htmlElement = HtmlConverter.ConvertToHtml(wDoc, settings);

                // Save the HTML to a file
                System.IO.File.WriteAllText(outputFile, htmlElement.ToStringNewLineOnAttributes());
            }
        }
    }
}

