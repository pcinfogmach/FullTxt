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
    public class ResultsViewer : Border
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
                this.Child = resultsWebView;
                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                string InstructionsPath = Path.Combine(appPath, "Resources", "הוראות חיפוש מתקדם.html");
                if (File.Exists(InstructionsPath)) { resultsWebView.Source = new Uri(InstructionsPath); }
                return;
            }
            else if (Result.TreeNode == null || string.IsNullOrEmpty(Result.Snippet)) { return; }

            string extension = Path.GetExtension(Result.TreeNode.Name);
            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                LoadPdfResult();
            }
            else
            {
                resultsWebView.Result = this.Result;
                if (this.Child != resultsWebView) { this.Child = resultsWebView; }
            }
        }

        void LoadPdfResult()
        {
            pdfPrevIewer.Viewer.FilePath = Result.TreeNode.Path;

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

            if (this.Child != pdfPrevIewer) { this.Child = pdfPrevIewer; }
        }
    }
}
