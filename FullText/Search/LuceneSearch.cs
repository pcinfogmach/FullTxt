using FullText.Tree;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Search
{
    public class LuceneSearch : LuceneIndexer
    {
        public List<ResultItem> Search(string queryText, List<TreeNode> checkedTreeNodes)
        {
            List<ResultItem> results = new List<ResultItem>();
            try
            {
                using (var directory = FSDirectory.Open(new DirectoryInfo(indexPath)))
                {
                    var searcher = new IndexSearcher(DirectoryReader.Open(directory));
                    Query query = parser.ParseSpanQuery(queryText, 2);
                    var topDocs = searcher.Search(query, int.MaxValue);
                    if (topDocs.ScoreDocs.Length == 0)
                    {
                        query = new FuzzyQuery(new Term("Content", queryText), 2);
                        topDocs = searcher.Search(query, int.MaxValue);
                    }

                    foreach (var scoreDoc in topDocs.ScoreDocs)
                    {
                        var path = searcher.Doc(scoreDoc.Doc).Get("Path");
                        var result = checkedTreeNodes.FirstOrDefault(node => node.Path == path);
                        if (result != null)
                        {
                            var snippets = Fragmentor.GetFragments(searcher, scoreDoc.Doc, query, analyzer);
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
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            return results;
        }
    }
}
