using FullText.Search;
using PdfiumPreViewer;
using PdfiumViewer.Enums;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
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
            }), DispatcherPriority.Background);
        }

        void LoadPdfResult()
        {
            pdfPrevIewer.Viewer.FilePath = Result.TreeNode.Path;
            pdfPrevIewer.Viewer.ZoomMode = PdfViewerZoomMode.FitWidth;

            string searchTerm = Result.MarkedTerm;
            pdfPrevIewer.Viewer.SearchManager.Search(null);
            pdfPrevIewer.Viewer.SearchManager.Search(searchTerm);
            pdfPrevIewer.Viewer.SearchManager.FindNext(true);

            for (int i = 0; i < Result.ResultNumber; i++)
            {
                pdfPrevIewer.Viewer.SearchManager.FindNext(true);
            }

            if (this.Child != pdfPrevIewer) { this.Child = pdfPrevIewer; }
        }

        static string TryGetExpandedTerm(string term, string snippet, bool direction)
        {
            int modifier = direction ? 1 : -1;

            int index = snippet.IndexOf(term);
            if (index == -1) return null; // Term not found in snippet

            int startIndex = index;
            int endIndex = index + term.Length;

            // Expand to the left or right depending on the direction
            while (true)
            {
                if (direction) // Expand to the right
                {
                    if (endIndex < snippet.Length && char.IsLetterOrDigit(snippet[endIndex]))
                    {
                        endIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                else // Expand to the left
                {
                    if (startIndex > 0 && char.IsLetterOrDigit(snippet[startIndex - 1]))
                    {
                        startIndex--;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // Return the expanded term
            return snippet.Substring(startIndex, endIndex - startIndex);
        }


    }
}
