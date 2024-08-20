using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace FullText.Search
{
    public class CustomHtmlHighlighter
    {
        public static List<string> GetSnippets(IndexSearcher searcher, Query query, int docId)
        {
            var spanText = searcher.Doc(docId).Get("Content").AsSpan();
            var queryStructure = GetBooLeanQueryStructure(searcher, query, docId);
            return FindAndHighlightAllBooleanMatches(spanText, queryStructure);
        }

        static List<List<ReadOnlyMemory<char>>> GetBooLeanQueryStructure(IndexSearcher searcher, Query query, int docId)
        {
            var explanation = searcher.Explain(query, docId);
            var termsMatched = new List<List<ReadOnlyMemory<char>>>();

            if (explanation.IsMatch)
            {
                if (!string.IsNullOrEmpty(explanation.Description))
                {
                    var splitString = explanation.Description.Split(new string[] { "])," }, StringSplitOptions.None);

                    foreach (string split in splitString)
                    {
                        MatchCollection matchCollection = Regex.Matches(split, @":([^ ]+)(\^|])");

                        var matchList = new List<ReadOnlyMemory<char>>();

                        foreach (Match match in matchCollection)
                        {
                            matchList.Add(match.Groups[1].Value.Trim('_', ' ').AsMemory());
                        }

                        termsMatched.Add(matchList);
                    }
                }
            }

            return termsMatched;
        }



        static List<string> FindAndHighlightAllBooleanMatches(ReadOnlySpan<char> spanText, List<List<ReadOnlyMemory<char>>> booleanQueryStructure, string highlightTag = "mark")
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
}
