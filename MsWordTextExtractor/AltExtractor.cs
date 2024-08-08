using DocumentFormat.OpenXml.Packaging;
using System.IO;
using System.Text;
using System.Xml;

namespace MsWordTextExtractor
{
    public static class AltDocxTextExtractor
    {
       

        public static string Extract(string filePath)
        {
            try
            {
                return ReadAllTextParts(filePath);
            }
            catch
            {
                return string.Empty;
            }
        }

        static string ReadAllTextParts(string filePath)
        {
            NameTable nameTable = new NameTable();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(nameTable);
            xmlNamespaceManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            StringBuilder stringBuilder = new StringBuilder();
            using (WordprocessingDocument wordprocessingDocument = WordprocessingDocument.Open(filePath, false))
            {
                var mainPart = wordprocessingDocument.MainDocumentPart;
                ReadMainPart(nameTable, xmlNamespaceManager, new StreamReader(mainPart.GetStream()).ReadToEnd(), stringBuilder);

                if (mainPart.FootnotesPart != null)
                {
                    ReadFootnotesPart(nameTable, xmlNamespaceManager, new StreamReader(mainPart.FootnotesPart.GetStream()).ReadToEnd(), stringBuilder);
                }

                if (mainPart.EndnotesPart != null)
                {
                    stringBuilder.AppendLine();
                    ReadFootnotesPart(nameTable, xmlNamespaceManager, new StreamReader(mainPart.EndnotesPart.GetStream()).ReadToEnd(), stringBuilder);
                }
            }
            return stringBuilder.ToString();
        }

        static void ReadMainPart(NameTable nameTable, XmlNamespaceManager xmlNamespaceManager, string xmlText, StringBuilder stringBuilder)
        {
            XmlDocument xmlDocument = new XmlDocument(nameTable);
            xmlDocument.LoadXml(xmlText);  
            ReadNodes(xmlDocument, xmlNamespaceManager, stringBuilder);
        }

        static void ReadFootnotesPart(NameTable nameTable, XmlNamespaceManager xmlNamespaceManager, string xmlText, StringBuilder stringBuilder)
        {
            XmlDocument xmlDocument = new XmlDocument(nameTable);
            xmlDocument.LoadXml(xmlText);

            XmlNodeList footnoteNodes = xmlDocument.SelectNodes("//w:footnote | .//w:endnote", xmlNamespaceManager);
            foreach (XmlNode footnoteNode in footnoteNodes)
            {
                string footnoteId = footnoteNode.Attributes["w:id"].Value;
                if (footnoteNode.Attributes["w:type"] != null && (footnoteNode.Attributes["w:type"].Value == "separator" || footnoteNode.Attributes["w:type"].Value == "continuationSeparator")) { continue; }
                stringBuilder.Append(footnoteId + ".");

                ReadNodes(footnoteNode, xmlNamespaceManager, stringBuilder);
                stringBuilder.AppendLine();
            }
        }

        static void ReadNodes(XmlNode xmlNode, XmlNamespaceManager xmlNamespaceManager, StringBuilder stringBuilder)
        {
            XmlNodeList textNodes = xmlNode.SelectNodes(".//w:p | .//w:t | .//w:tab | .//w:br | .//w:footnoteReference", xmlNamespaceManager);
            foreach (XmlNode textNode in textNodes)
            {
                switch (textNode.Name)
                {
                    case "w:p":
                        stringBuilder.AppendLine(textNode.InnerText);
                        break;
                    
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
                }
            }
        }       
    }
}
