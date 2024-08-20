using FullText.Search.Tests;
using FullText.Tree;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Spans;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Search
{
    public class LuceneSearch : LuceneBase
    {
        public int slop = Properties.Settings.Default.DistanceBetweenSearchWords;
        public List<ResultItem> results = new List<ResultItem>();
        int totalSum;

        public async Task<List<ResultItem>> Search(string queryText, List<TreeNode> checkedTreeNodes)
        {
            results = new List<ResultItem>();
            try
            {
                totalSum = 0;
                return await Task.Run(() =>
                {
                    //try
                    //{
                    using (var directory = FSDirectory.Open(new DirectoryInfo(indexPath)))
                    {
                        IndexSearcher searcher = new IndexSearcher(DirectoryReader.Open(directory))
                        {
                            Similarity = new CustomSimilarity()
                        };


                        Query query = parser.ParseSpanQuery(queryText, slop);
                        var topDocs = PerformSearch(query, searcher, ref queryText, 2);

                        foreach (var scoreDoc in topDocs.ScoreDocs)
                        {
                            GetResults(query, queryText, checkedTreeNodes, searcher, scoreDoc.Doc);
                        }
                    }

                    return results;
                });
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); return results; }
        }

        public TopDocs PerformSearch(Query query, IndexSearcher searcher, ref string queryText, int fuzzyModifier)
        {
            TopDocs topDocs = null;

            topDocs = searcher.Search(query, int.MaxValue);

            if (topDocs.ScoreDocs.Length == 0 && fuzzyModifier > 0)
            {
                queryText = Regex.Replace(queryText, @"~\d?", "");
                queryText = Regex.Replace(queryText, @"(\p{IsHebrew}+)(\W|$)", "$1~1$2");
                query = parser.ParseSpanQuery(queryText, slop);
                topDocs = searcher.Search(query, int.MaxValue);
            }

            if (topDocs.ScoreDocs.Length == 0 && fuzzyModifier > 1)
            {
                queryText = Regex.Replace(queryText, @"~\d?", "");
                queryText = Regex.Replace(queryText, @"(\p{IsHebrew}+)(\W|$)", "$1~2$2");
                query = parser.ParseSpanQuery(queryText, slop);
                topDocs = searcher.Search(query, int.MaxValue);
            }

            return topDocs;
        }

        void GetResults(Query query, string queryText, List<TreeNode> checkedTreeNodes, IndexSearcher searcher, int scoreDocId)
        {
            var path = searcher.Doc(scoreDocId).Get("Path");
            var result = checkedTreeNodes.FirstOrDefault(node => node.Path == path);
            if (result != null)
            {
                //var snippets = CustomHtmlHighlighter.GetSnippets(searcher, query, scoreDocId);
                var snippets = Fragmentor.GetFragments(searcher, scoreDocId, parser.ParseSpanQuery(queryText, slop), analyzer);

                for (int i = 0; i < snippets.Count; i++)
                {
                    totalSum++;
                    results.Add(new ResultItem
                    {
                        TreeNode = result,
                        GroupName = result.Name,
                        Title = totalSum + ". " + result.Name,
                        Snippet = snippets[i],
                        ResultNumber = i,
                        TotalRelativeResults = snippets.Count,
                    });
                }
            }
        }
    }
}
