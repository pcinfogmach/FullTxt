using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

internal class CustomHighlighter5
{
    public static List<string> FindAndHighlightAllBooleanMatches(ReadOnlySpan<char> spanText, List<List<ReadOnlyMemory<char>>> booleanQueryStructure, string highlightTag = "mark")
    {
        List<string> highlightedSnippets = new List<string>();
        List<KeyValuePair<int, int>> indexPairs = new List<KeyValuePair<int, int>>();

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
                    indexPairs.Add(new KeyValuePair<int, int>(bestMatchIndex, bestMatchIndex + bestMatchLength - 1));
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
        StringBuilder stringBuilder = new StringBuilder();
        int lastMatchEnd = 0;

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
            string highlightedTerm = spanText.Slice(startIndex, endIndex - startIndex + 1).ToString();
            stringBuilder.Append($"<{highlightTag}>{highlightedTerm}</{highlightTag}>");

            lastMatchEnd = endIndex + 1;
        }

        // Append any remaining text after the last highlighted term
        if (lastMatchEnd < spanText.Length)
        {
            stringBuilder.Append(spanText.Slice(lastMatchEnd).ToString());
        }

        return stringBuilder.ToString();
    }
}


//if (bestMatchIndex >= 0 && bestMatchLength > 0)
//{
//    int matchEnd = bestMatchIndex + bestMatchLength - 1;

//    // Check if this match overlaps with the last match, if so, extend the last match
//    if (indexPairs.Count > 0 && indexPairs.Last().Value >= bestMatchIndex)
//    {
//        indexPairs[indexPairs.Count - 1] = new KeyValuePair<int, int>(indexPairs.Last().Key, matchEnd);
//    }
//    else
//    {
//        if (indexPairs.Count > 0)
//        {
//            // Generate the snippet for the previous set of matches
//            highlightedSnippets.Add(CreateHighlightedSnippet(spanText, indexPairs, highlightTag));
//            indexPairs.Clear();
//        }
//        indexPairs.Add(new KeyValuePair<int, int>(bestMatchIndex, matchEnd));
//    }

//    currentPosition = matchEnd; // Skip past this match
//}
