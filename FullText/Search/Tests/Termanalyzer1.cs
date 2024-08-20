using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FullText.Search.Tests
{
    internal class TermAnalyzer1 : LuceneBase
    {
        public void AnalyzeTermsUsingPostingsEnum()
        {
            int maxProximity = 20;
            // List of terms to search for
            List<string> termsToSearch = new List<string> { "מדוע", "מותר", "להניח", "תפילין" };

            // Open the directory
            var directory = FSDirectory.Open(indexPath);

            // Create an IndexReader
            var reader = DirectoryReader.Open(directory);

            // Access the fields in the index
            var fields = MultiFields.GetFields(reader);

            if (fields == null) return;

            // Dictionary to hold the set of document IDs for each term and their positions
            var docsContainingTerms = new Dictionary<string, Dictionary<int, List<int>>>();

            foreach (var termText in termsToSearch)
            {
                docsContainingTerms[termText] = new Dictionary<int, List<int>>();

                foreach (var field in fields.Cast<string>())
                {
                    var terms = fields.GetTerms(field);
                    if (terms == null) continue;

                    var termsEnum = terms.GetEnumerator();
                    if (termsEnum.SeekExact(new BytesRef(termText)))
                    {
                        var docsAndPositionsEnum = termsEnum.DocsAndPositions(null, null);
                        if (docsAndPositionsEnum != null)
                        {
                            while (docsAndPositionsEnum.NextDoc() != DocIdSetIterator.NO_MORE_DOCS)
                            {
                                int docId = docsAndPositionsEnum.DocID;
                                int freq = docsAndPositionsEnum.Freq;
                                var positions = new List<int>();

                                for (int i = 0; i < freq; i++)
                                {
                                    positions.Add(docsAndPositionsEnum.NextPosition());
                                }

                                docsContainingTerms[termText][docId] = positions;
                            }
                        }
                    }
                }
            }

            // Find the common documents and verify proximity
            var commonDocIds = docsContainingTerms.Values
                .SelectMany(dict => dict.Keys)
                .GroupBy(id => id)
                .Where(group => group.Count() == termsToSearch.Count)
                .Select(group => group.Key);

            // Output the details for documents that contain all terms within the specified proximity
            foreach (var docId in commonDocIds)
            {
                bool isProximityMatch = true;

                for (int i = 0; i < termsToSearch.Count - 1; i++)
                {
                    var firstTermPositions = docsContainingTerms[termsToSearch[i]][docId];
                    var secondTermPositions = docsContainingTerms[termsToSearch[i + 1]][docId];

                    bool foundProximity = firstTermPositions
                        .Any(pos1 => secondTermPositions.Any(pos2 => Math.Abs(pos2 - pos1) <= maxProximity));

                    if (!foundProximity)
                    {
                        isProximityMatch = false;
                        break;
                    }
                }

                if (isProximityMatch)
                {
                    Console.WriteLine($"Document ID: {docId} contains all terms within the specified proximity of {maxProximity}.");
                    foreach (var termText in termsToSearch)
                    {
                        foreach (var field in fields.Cast<string>())
                        {
                            var terms = fields.GetTerms(field);
                            if (terms == null) continue;

                            var termsEnum = terms.GetEnumerator();
                            if (termsEnum.SeekExact(new BytesRef(termText)))
                            {
                                var docsAndPositionsEnum = termsEnum.DocsAndPositions(null, null);
                                if (docsAndPositionsEnum == null) continue;

                                while (docsAndPositionsEnum.NextDoc() != DocIdSetIterator.NO_MORE_DOCS)
                                {
                                    if (docsAndPositionsEnum.DocID != docId) continue;

                                    int freq = docsAndPositionsEnum.Freq;
                                    var positions = new List<int>();

                                    for (int i = 0; i < freq; i++)
                                    {
                                        positions.Add(docsAndPositionsEnum.NextPosition());
                                    }

                                    Console.WriteLine($"Term '{termText}' found in field '{field}' with Term Frequency: {freq}, Positions: {string.Join(", ", positions)}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}