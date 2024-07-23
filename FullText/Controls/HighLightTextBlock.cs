using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace FullText.Controls
{
    public class HighLightTextBlock : TextBlock
    {
        public static readonly DependencyProperty HighlightedTextProperty =
            DependencyProperty.Register("HighlightedText", typeof(string), typeof(HighLightTextBlock), new PropertyMetadata(string.Empty, OnHighlightedTextChanged));

        public string HighlightedText
        {
            get { return (string)GetValue(HighlightedTextProperty); }
            set { SetValue(HighlightedTextProperty, value); }
        }

        private static void OnHighlightedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as HighLightTextBlock;
            control?.UpdateText();
        }

        private void UpdateText()
        {
            Inlines.Clear();
            if (string.IsNullOrEmpty(HighlightedText))
            {
                return;
            }

            string pattern = @"<mark>(.*?)</mark>";

            int lastPos = 0;
            var matches = Regex.Matches(HighlightedText, pattern);

            foreach (Match match in matches)
            {
                // Add text before the match
                if (match.Index > lastPos)
                {
                    Inlines.Add(new Run(HighlightedText.Substring(lastPos, match.Index - lastPos)));
                }

                // Add highlighted text
                if (match.Groups[1].Success)
                {
                    Inlines.Add(new Run(match.Groups[1].Value) { Foreground = Brushes.Magenta });
                }

                lastPos = match.Index + match.Length;
            }

            // Add remaining text after the last match
            if (lastPos < HighlightedText.Length)
            {
                Inlines.Add(new Run(HighlightedText.Substring(lastPos)));
            }
        }
    }
}

