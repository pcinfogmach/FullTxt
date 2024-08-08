using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordInterop = Microsoft.Office.Interop.Word;

namespace MsWordTextExtractor
{
    public static class AltWordInteropExtractor
    {
        public static string Extract(string filePath)
        {
            StringBuilder stringBuilder = new StringBuilder();
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

                    var selection = doc.Application.Selection;

                    var bodyRange = doc.Content;
                    stringBuilder.Append(bodyRange.Text);

                    var footnotes = bodyRange.Footnotes.Cast<Footnote>().ToList();
                    Range footnotesRange = footnotes.First().Range;
                    footnotesRange.End = footnotes.Last().Range.End;
                    stringBuilder.AppendLine(footnotesRange.Text);

                    var endnotes = bodyRange.Endnotes.Cast<Endnote>().ToList();
                    Range endnotesRange = endnotes.First().Range;
                    endnotesRange.End = endnotes.Last().Range.End;
                    stringBuilder.AppendLine(endnotesRange.Text);

                    if (doc != null && !isFileAlreadyOpen) doc.Close(WordInterop.WdSaveOptions.wdDoNotSaveChanges);
                }

                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return string.Empty;
            }
        }
    }
}
