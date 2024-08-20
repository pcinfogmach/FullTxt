using System;
using System.Collections.Generic;

namespace FullText.Search.Tests
{
    internal class FindAllIndexes2
    {
        public static List<int> FindAll(string text, string searchText)
        {
            var spanText = text.AsSpan();
            var spanSearchText = searchText.AsSpan();
            List<int> positions = new List<int>();

            int index = 0;
            while (index <= spanText.Length - spanSearchText.Length)
            {
                bool matchFound = true;

                for (int i = 0; i < spanSearchText.Length; i++)
                {
                    index++;
                    if (spanText[index + i] != spanSearchText[i])
                    {
                        matchFound = false;
                        break;
                    }
                }

                if (matchFound) 
                    positions.Add(index);
            }

            return positions;
        }

    }
}
