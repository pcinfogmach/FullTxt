using FullText.Helpers;
using FullText.Search;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using org.apache.poi.ss.formula.functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Controls
{
    internal class ResultsWebView : WebView2
    {
        public static readonly DependencyProperty ResultProperty =
                    DependencyProperty.Register("Result", typeof(ResultItem), typeof(ResultsWebView), new PropertyMetadata(new ResultItem(), OnResultChanged));

        public ResultItem Result
        {
            get { return (ResultItem)GetValue(ResultProperty); }
            set { SetValue(ResultProperty, value); }
        }

        private static string currentPath = string.Empty;

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ResultsWebView;
            control?.LoadResult();
        }

        public async void LoadResult()
        {
            if (Result == null) { return; }
            if (Result.TreeNode.Path == currentPath)
            {
                FindSnippet();
                return;
            }

            currentPath = Result.TreeNode.Path;

            this.Visibility = Visibility.Hidden;
            Source = new Uri("about:blank");


            if (Result.TreeNode.Path.IsWordDocumentFile())
            {
                string tempPath = Result.TreeNode.Path;
                tempPath = await Task.Run(() =>
                {
                    return HtmlConverter.Convert(tempPath);
                });
                Source = new Uri(tempPath);
            }
            else if (Result.TreeNode.Path.IsCompressedFile())
            {
                string tempPath = Result.TreeNode.Path;
                tempPath = await Task.Run(() =>
                {
                    return HtmlConverter.TikaConverter(tempPath);
                });
                Source = new Uri(tempPath);
            }
            else { Source = new Uri(Result.TreeNode.Path); }

            if (Result.TreeNode.Path.IsPdfFile()) { this.Visibility = Visibility.Visible; return; }         
            
            await EnsureCoreWebView2Async(null);
            CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
        }

        private void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            FindSnippet();
            ExecuteScriptAsync("document.dir = 'auto';");
            this.Visibility = Visibility.Visible;
            CoreWebView2.DOMContentLoaded -= CoreWebView2_DOMContentLoaded;
        }

        async void FindSnippet()
        {
            string snippet = Regex.Replace(Result.Snippet, @"</?mark>", "");
            string markedText = Regex.Match(Result.Snippet, @"<mark>(.*)</mark>").Value;
            markedText = Regex.Replace(markedText, @"</?mark>", "");

            var lines = snippet.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string lineContainingMarkedText = lines.FirstOrDefault(line => line.Contains(markedText));
            
            await ExecuteScriptAsync($@"
window.getSelection().removeAllRanges(); 
window.find(`{lineContainingMarkedText}`);
window.getSelection().collapseToStart();
window.find(`{markedText}`)
");
        }
    }
}
