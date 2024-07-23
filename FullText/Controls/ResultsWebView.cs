using FullText.Helpers;
using FullText.Search;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
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

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ResultsWebView;
            control?.LoadResult();
        }

        public async void LoadResult()
        {
            if (Result == null) { return; }
            
            this.Visibility = Visibility.Hidden;
            Source = new Uri("about:blank");

            string[] MsWordExtensions = { ".doc", ".docm", ".docx", ".dotx", ".dotm", ".dot", ".odt", ".rtf" };
            string extension = Path.GetExtension(Result.TreeNode.Name);

            if (MsWordExtensions.Contains(extension))
            {
                string tempPath = WordToHtmlConverter.Convert(Result.TreeNode.Path);
                Source = new Uri(tempPath); 
            }
            else { Source = new Uri(Result.TreeNode.Path); }

            if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) { this.Visibility = Visibility.Visible; return; }         
            
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
            string markedText = Regex.Match(Result.Snippet, @"<mark>(.*?)</mark>").Value;
            markedText = Regex.Replace(markedText, @"</?mark>", "");

            var lines = snippet.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string lineContainingMarkedText = lines.FirstOrDefault(line => line.Contains(markedText));
            if (lineContainingMarkedText != null)
            {
                await FindTextAsync(lineContainingMarkedText);
            }
            else
            {
                await FindTextAsync(markedText);
            }
        }

        private async Task FindTextAsync(string searchTerm)
        {
            string script = $@"
            const targetString = `{searchTerm}`;
            const content = document.body.innerHTML;
            const index = content.indexOf(targetString);

            if (index !== -1) {{
               const highlightedText = content.substring(0, index) + 
                                '<span style=""background-color:lightgray"">' + `{Result.Snippet}` + 
                                '</span>' + content.substring(index + targetString.length);

                document.body.innerHTML = highlightedText;
                document.querySelector('mark').scrollIntoView({{ block: ""center"" }});
            }} else {{
                window.find(`{searchTerm}`);
            }}";

            await ExecuteScriptAsync(script);

        }
    }
}
