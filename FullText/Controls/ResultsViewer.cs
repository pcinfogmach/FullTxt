using FullText.Search;
using PdfiumPreViewer;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FullText.Controls
{
    public class ResultsViewer : ContentControl
    {
        PdfiumPreViewer.PreViewer pdfPrevIewer = new PdfiumPreViewer.PreViewer();
        ResultsWebView resultsWebView = new ResultsWebView();

        public static readonly DependencyProperty ResultProperty =
                    DependencyProperty.Register("Result", typeof(ResultItem), typeof(ResultsViewer), new PropertyMetadata(new ResultItem(), OnResultChanged));

        public ResultItem Result
        {
            get { return (ResultItem)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ResultsViewer;
            control?.LoadResult();
        }

        public void LoadResult()
        {
            if (Result == null)
            {
                this.Content = null;
                return;
            }

            string extension = Path.GetExtension(Result.TreeNode.Name);
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                LoadPdfResult();
            }
            else
            {
                Content = resultsWebView;
                resultsWebView.Result = this.Result;
            }
        }

        void LoadPdfResult()
        {
            pdfPrevIewer.Viewer.OpenPdf(new FileStream(Result.TreeNode.Path, FileMode.Open, FileAccess.Read, FileShare.Read));
            this.Content = pdfPrevIewer;

            string snippet = Regex.Replace(Result.Snippet, @"</?mark>", "");
            string markedText = Regex.Match(Result.Snippet, @"<mark>(.*?)</mark>").Value;
            markedText = Regex.Replace(markedText, @"</?mark>", "");

            var lines = snippet.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string lineContainingMarkedText = lines.FirstOrDefault(line => line.Contains(markedText));


            if (lineContainingMarkedText != null)
            {
                pdfPrevIewer.Viewer.InitialeSearchTerm = lineContainingMarkedText;
            }
            else
            {
                pdfPrevIewer.Viewer.InitialeSearchTerm = markedText;
            }
        }
    }
}
