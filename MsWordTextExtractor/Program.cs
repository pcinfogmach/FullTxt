using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MsWordTextExtractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "C:\\Users\\Admin\\Desktop\\וידאו מספק דרך רבת - Copy.doc";
            //string filePath = "C:\\Users\\Admin\\Documents\\גמ''ח\\ספריה תורנית\\אוצר התורה מיר - מאגר מ''מ\\1ברכות\\דור המלקטים או''ח קנז-רמ.doc";
            string convertedDoc = filePath + ".txt";
            File.WriteAllText(convertedDoc, NpoiDocExtractor.ExtractTextFromDoc(filePath));
            //XMlPowerToolsExtractor.ConvertDocxToHtml(filePath, convertedDoc);
            System.Diagnostics.Process.Start(convertedDoc);
        }

      
    }
}
