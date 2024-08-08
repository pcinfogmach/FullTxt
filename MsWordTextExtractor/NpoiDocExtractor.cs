using NPOI.HWPF.Extractor;
using NPOI.HWPF;
using System.IO;

namespace MsWordTextExtractor
{
    internal class NpoiDocExtractor
    {
        public static string ExtractTextFromDoc(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                HWPFDocument doc = new HWPFDocument(fileStream);
                WordExtractor extractor = new WordExtractor(doc);
                return extractor.Text;
            }
        }
    }
}
