using Microsoft.Web.WebView2.Wpf;
using System.Windows;

namespace FullText.Controls
{
    internal class SnippetsViewer : WebView2
    {
        public static readonly DependencyProperty ContentProperty =
                    DependencyProperty.Register("Result", typeof(string), typeof(SnippetsViewer), new PropertyMetadata(string.Empty, OnConetntChanged));

        public string Content
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        private static void OnConetntChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SnippetsViewer;
            control?.LoadContent();
        }

        async void LoadContent()
        {
            if (Content != null)
            {
                await EnsureCoreWebView2Async(null);
                NavigateToString(Content);
            }
        }    
    }
}
