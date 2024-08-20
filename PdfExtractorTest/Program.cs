using FullText.Helpers;
using sun.swing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfExtractorTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            

            // Define the new path for the file on the desktop

            //string content = TextExtractor.ReadText(originalPath);

            //// Write the content to the file on the desktop
            //File.WriteAllText(path, content);

            string fileExtesnion = ".txt";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string originalPath = "C:\\Users\\Admin\\Desktop\\TextSeekersTests\\Pdf\\הגדה של פסח מדרש הגדה להמלבי''ם.pdf";
            string content = new XpdfNet.XpdfHelper().ToText(originalPath);
            string path = Path.Combine(desktopPath, "xpdfConvert" + fileExtesnion);
            File.WriteAllText(path, content);

            //string fileName = "וידאו מספק דרך רבת - Copy.rtf";
            //string path = Path.Combine(desktopPath, fileName);

            //string rtfContent = File.ReadAllText(path);
            //RichTextBox richTextBox = new RichTextBox
            //{
            //    Rtf = rtfContent
            //};

            //string plainText = richTextBox.Text;

            //File.WriteAllText(path +".txt", plainText);
        }
    }
}
