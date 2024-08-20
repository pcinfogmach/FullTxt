using System;
using System.Collections.Generic;
using System.Linq;

namespace FullText.Search.Tests
{
    internal class FindAllIndexes3
    {

        public static List<int> FindAll(string text, List<List<string>> searchTextSynonymsValues)
        {
            if (searchTextSynonymsValues.Count == 0)
                return new List<int>();

            var spanText = text.AsSpan();
            List<int> positions = new List<int>();

            int currentIndex = 0;
            int wordIndex = 0;

            while (currentIndex < spanText.Length)
            {
                // Skip any leading spaces
                while (currentIndex < spanText.Length && spanText[currentIndex] == ' ')
                {
                    currentIndex++;
                }

                if (currentIndex >= spanText.Length)
                {
                    break;
                }

                // Find the end of the current word
                int wordEndIndex = spanText.Slice(currentIndex).IndexOf(' ');
                if (wordEndIndex == -1)
                {
                    wordEndIndex = spanText.Length - currentIndex;
                }

                var wordSpan = spanText.Slice(currentIndex, wordEndIndex);
                bool matchFound = true;

                for (int i = 0; i < searchTextSynonymsValues.Count; i++)
                {
                    if (currentIndex >= spanText.Length)
                    {
                        matchFound = false;
                        break;
                    }

                    var synonymList = searchTextSynonymsValues[i];
                    if (!synonymList.Contains(wordSpan.ToString(), StringComparer.OrdinalIgnoreCase))
                    {
                        matchFound = false;
                        break;
                    }

                    wordIndex = currentIndex;

                    // Move to the next word
                    currentIndex += wordSpan.Length;

                    // Skip spaces after the word
                    while (currentIndex < spanText.Length && spanText[currentIndex] == ' ')
                    {
                        currentIndex++;
                    }

                    if (currentIndex < spanText.Length)
                    {
                        wordEndIndex = spanText.Slice(currentIndex).IndexOf(' ');
                        if (wordEndIndex == -1)
                        {
                            wordEndIndex = spanText.Length - currentIndex;
                        }
                        wordSpan = spanText.Slice(currentIndex, wordEndIndex);
                    }
                }

                if (matchFound)
                {
                    positions.Add(wordIndex);
                }

                // Move past the current word
                currentIndex += wordSpan.Length;
            }

            return positions;
        }
    }
}
