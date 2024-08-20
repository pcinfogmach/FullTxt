using J2N.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace FullText.Search.Tests
{
    internal class CustomHtmlHighlighter4
    {
        private static string FindAndHighlightAllBooleanMatches(ReadOnlySpan<char> spanText, List<List<string>> booleanQueryStructure, string highlightTag = "mark")
        {
            StringBuilder highlightedText = new StringBuilder(spanText.Length);
            List<int> matchPositions = new List<int>();
            int lastMatchEnd = 0;

            for (int i = 0; i <= spanText.Length; i++)
            {
                bool allGroupsMatch = true;
                int currentPosition = i;
                int startPosition = i;

                foreach (var synonymGroup in booleanQueryStructure)
                {
                    bool groupMatch = false;

                    foreach (var synonym in synonymGroup)
                    {
                        var spanSynonym = synonym.AsSpan();

                        if (currentPosition <= spanText.Length - spanSynonym.Length &&
                            spanText.Slice(currentPosition, spanSynonym.Length).SequenceEqual(spanSynonym))
                        {
                            groupMatch = true;
                            currentPosition += spanSynonym.Length;
                            break;
                        }
                    }

                    if (!groupMatch)
                    {
                        allGroupsMatch = false;
                        break;
                    }
                }

                if (allGroupsMatch)
                {
                    // Add the text before the match
                    highlightedText.Append(spanText.Slice(lastMatchEnd, startPosition - lastMatchEnd).ToString());

                    // Add the matched text with highlight tags
                    highlightedText.Append($"<{highlightTag}>");
                    highlightedText.Append(spanText.Slice(startPosition, currentPosition - startPosition).ToString());
                    highlightedText.Append($"</{highlightTag}>");

                    lastMatchEnd = currentPosition;
                    i = currentPosition - 1; // Move the index forward to continue after the current match
                }
            }

            // Append the remaining part of the text after the last match
            highlightedText.Append(spanText.Slice(lastMatchEnd).ToString());

            return highlightedText.ToString();
        }
    }
}
