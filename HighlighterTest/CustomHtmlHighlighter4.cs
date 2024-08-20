using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

public class CustomHtmlHighlighter4
{
    public static List<string> FindAndHighlightAllBooleanMatches(ReadOnlySpan<char> spanText, List<List<ReadOnlyMemory<char>>> booleanQueryStructure, string highlightTag = "mark")
    {
        List<string> highlightedSnippets = new List<string>();
        List<KeyValuePair<int, int>> indexPairs = new List<KeyValuePair<int, int>>();
        
        int lastMatchEnd = 0;

        for (int currentPosition = 0; currentPosition < spanText.Length; currentPosition++)
        {
            if (!char.IsLetterOrDigit(spanText[currentPosition])) continue;

            var slice = spanText.Slice(currentPosition);
            int bestMatchIndex = -1;
            int bestMatchLength = 0;

            // Iterate over each group to find the best match
            foreach (var group in booleanQueryStructure)
            {
                foreach (var synonym in group)
                {
                    if (slice.StartsWith(synonym.Span, StringComparison.OrdinalIgnoreCase))
                    {
                        if (synonym.Length > bestMatchLength)
                        {
                            bestMatchIndex = currentPosition;
                            bestMatchLength = synonym.Length;
                        }
                    }
                }

                if (bestMatchIndex >= 0)
                {
                    currentPosition += bestMatchLength - 1;
                    indexPairs.Add(new KeyValuePair<int, int>(bestMatchIndex, bestMatchIndex + bestMatchLength -1));
                    break;
                }
                  
            }

            if (indexPairs.Count >= booleanQueryStructure.Count)
            {
                highlightedSnippets.Add(CreateHighlightedSnippet(spanText, indexPairs, highlightTag));
                indexPairs = new List<KeyValuePair<int, int>>();
            }
        }

        // Create and add the last highlighted snippet if any matches remain
        

        return highlightedSnippets;
    }

    private static string CreateHighlightedSnippet(ReadOnlySpan<char> spanText, List<KeyValuePair<int, int>> indexPairs, string highlightTag)
    {
        StringBuilder stringBuilder = new StringBuilder(spanText.Length);
        int lastMatchEnd = Math.Max(indexPairs[0].Key - 75, 0);

        foreach (var indexPair in indexPairs)
        {
            int startIndex = indexPair.Key;
            int endIndex = indexPair.Value;

            // Append text before the highlighted term
            if (startIndex > lastMatchEnd)
            {
                stringBuilder.Append(spanText.Slice(lastMatchEnd, startIndex - lastMatchEnd).ToString());
            }

            // Append highlighted term
            var highlightSlice = spanText.Slice(startIndex, endIndex - startIndex + 1);
            stringBuilder.Append($"<{highlightTag}>{highlightSlice.ToString()}</{highlightTag}>");

            lastMatchEnd = endIndex + 1;
        }

        // Append any remaining text after the last match
        if (lastMatchEnd < spanText.Length)
        {
            stringBuilder.Append(spanText.Slice(lastMatchEnd, Math.Min(75, spanText.Length - lastMatchEnd)).ToString());
        }

        return stringBuilder.ToString();
    }

}
