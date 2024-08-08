using FullText.Tree;
using Lucene.Net.Index;
using Lucene.Net.Search;
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

        public async Task<List<ResultItem>> Search(string queryText, List<TreeNode> checkedTreeNodes)
        {
            return await Task.Run(() =>
            {
                results = new List<ResultItem>();
                try
                {
                    using (var directory = FSDirectory.Open(new DirectoryInfo(indexPath)))
                    {
                        IndexSearcher searcher = new IndexSearcher(DirectoryReader.Open(directory));
                        var topDocs = PerformSearch(searcher, ref queryText, 2);

                        foreach (var scoreDoc in topDocs.ScoreDocs)
                        {
                            GetResults(queryText, checkedTreeNodes, searcher, scoreDoc.Doc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                return results;
            });
        }

        void GetResults(string queryText, List<TreeNode> checkedTreeNodes, IndexSearcher searcher, int scoreDocId)
        {
            var path = searcher.Doc(scoreDocId).Get("Path");
            var result = checkedTreeNodes.FirstOrDefault(node => node.Path == path);
            if (result != null)
            {
                var snippets = Fragmentor.GetFragments(searcher, scoreDocId, parser.ParseSpanQuery(queryText, slop), analyzer);

                foreach (var snippet in snippets)
                {
                    results.Add(new ResultItem
                    {
                        TreeNode = result,
                        Snippet = snippet
                    });
                }
            }
        }

        public TopDocs PerformSearch(IndexSearcher searcher, ref string queryText, int fuzzyModifier)
        {
            TopDocs topDocs = null;
            Lucene.Net.Search.Query query = parser.ParseSpanQuery(queryText, slop);

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
    }
}
