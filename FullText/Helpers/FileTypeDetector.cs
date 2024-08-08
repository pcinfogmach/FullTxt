using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Helpers
{
    public static class FileTypeDetector
    {
        private static readonly string[] CompressedExtensions = new string[]{
        ".zip", ".gz", ".rar", ".7z", ".tar", ".bz2", ".xz", ".lzma", ".tgz", ".tbz2" };
        
        private static readonly string[] WordDocumentExtensions = new string[]{
        ".doc", ".docm", ".docx", ".dotx", ".dotm", ".dot", ".odt", ".rtf" };

        public static bool IsCompressedFile(this string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return CompressedExtensions.Contains(extension);
        } 
        
        public static bool IsWordDocumentFile(this string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return WordDocumentExtensions.Contains(extension);
        }

        public static bool IsPdfFile(this string filePath)
        {
            string extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return extension == ".pdf";
        }
    }
}
