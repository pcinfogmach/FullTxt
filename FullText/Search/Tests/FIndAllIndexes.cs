using System;
using System.Collections.Generic;
using System.Linq;

namespace FullText.Helpers
{
    public static class FindAllIndexes
    {
        public static List<int> FindAll(string text, string searchText)
        {
            var spanText = text.AsSpan();
            var spanSearch = searchText.AsSpan();
            List<int> positions = new List<int>();

            int index = spanText.IndexOf(spanSearch);
            while (index != -1)
            {
                int adjustedIndex = index + (positions.Count > 0 ? positions[positions.Count - 1] + 1 : 0);
                positions.Add(adjustedIndex);
                spanText = spanText.Slice(index + 1);
                index = spanText.IndexOf(spanSearch);
            }

            return positions;
        }

        public static IEnumerable<int> YieldAllIndexes(string text, string term)
        {
            var spanText = text.AsSpan();
            var spanSearch = term.AsSpan();
            int offset = 0;

            List<int> indexes = new List<int>();
            int index = spanText.IndexOf(spanSearch);
            while (index != -1)
            {
                indexes.Add(index + offset);
                offset += index + 1;
                spanText = spanText.Slice(index + 1);
                index = spanText.IndexOf(spanSearch);
            }

            return indexes;
        }

        public static List<string> CreateHighLightedSnippets(string text, string[] terms, int distanceBetweenTerms)
        {
            int snippetLength = 150 + terms.Length * 10; // Adjusted snippet length
            List<string> snippets = new List<string>();
            int currentIndex = 0;
            ReadOnlySpan<char> spanText = text.AsSpan();

            while (currentIndex < spanText.Length)
            {
                // Find the next occurrence of each term starting from currentIndex
                List<int> termIndexes = new List<int>();
                foreach (var term in terms)
                {
                    ReadOnlySpan<char> spanTerm = term.AsSpan();
                    int index = spanText.Slice(currentIndex).IndexOf(spanTerm, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        termIndexes.Add(currentIndex + index);
                    }
                }

                if (termIndexes.Count == 0)
                {
                    break; // No more terms found
                }

                // Sort termIndexes to find the first and last occurrence
                termIndexes.Sort();
                int snippetStart = Math.Max(0, termIndexes[0] - snippetLength / 2);
                int snippetEnd = Math.Min(snippetStart + snippetLength, spanText.Length);

                // Ensure that all terms are included in the snippet
                for (int i = termIndexes.Count - 1; i >= 0; i--)
                {
                    if (termIndexes[i] > snippetEnd)
                    {
                        snippetEnd = Math.Min(termIndexes[i] + snippetLength / 2, spanText.Length);
                        snippetStart = Math.Max(0, snippetEnd - snippetLength);
                    }
                }

                // Extract the snippet and convert it back to a string
                string snippet = spanText.Slice(snippetStart, snippetEnd - snippetStart).ToString();
                snippets.Add(HighlightTerms(snippet, terms));

                // Move currentIndex past this snippet
                currentIndex = snippetEnd;
            }

            return snippets;
        }

        private static string HighlightTerms(string text, string[] terms)
        {
            string highlightedText = text;
            foreach (var term in terms)
            {
                string highlightTag = $"<mark>{term}</mark>";
                highlightedText = highlightedText.Replace(term, highlightTag);
            }
            return highlightedText;
        }

    }
}
