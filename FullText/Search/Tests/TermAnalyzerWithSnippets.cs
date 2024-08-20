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
    internal class TermAnalyzerWithSnippets : LuceneBase
    {
        public void AnalyzeTermsUsingPostingsEnum()
        {
            int maxProximity = 20;
            int snippetLength = 50; // Length of the snippet around the term position
            List<string> termsToSearch = new List<string> { "מדוע", "מותר", "להניח", "תפילין" };

            var directory = FSDirectory.Open(indexPath);
            var reader = DirectoryReader.Open(directory);
            var fields = MultiFields.GetFields(reader);

            if (fields == null) return;

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

            var commonDocIds = docsContainingTerms.Values
                .SelectMany(dict => dict.Keys)
                .GroupBy(id => id)
                .Where(group => group.Count() == termsToSearch.Count)
                .Select(group => group.Key);

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

                    // Extract snippets
                    var doc = reader.Document(docId);
                    string content = doc.Get("Content"); // Assuming the field containing the text is named "content"
                    if (!string.IsNullOrEmpty(content)) 
                    {
                        var snippets = new List<string>();

                        foreach (var termText in termsToSearch)
                        {
                            foreach (var position in docsContainingTerms[termText][docId])
                            {
                                int start = Math.Max(0, position - snippetLength / 2);
                                int end = Math.Min(content.Length, start + snippetLength);
                                string snippet = content.Substring(start, end - start);
                                snippets.Add(snippet);
                            }
                        }

                        // Output snippets
                        foreach (var snippet in snippets)
                        {
                            Console.WriteLine($"Snippet: {snippet}...");
                        }
                    }                    
                }
            }
        }
    }
}
