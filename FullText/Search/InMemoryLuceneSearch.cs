using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using FullText.Helpers;
using System.Collections.Generic;

namespace FullText.Search
{
    public class InMemoryLuceneSearch : LuceneSearch
    {
        private RAMDirectory ramDirectory;
        public List<ResultItem> Search(string file, string queryText)
        {
            using (ramDirectory = new RAMDirectory())
            {
                IndexFile(file);
                var searcher = new IndexSearcher(DirectoryReader.Open(ramDirectory));
                Query query = parser.ParseSpanQuery(queryText, slop);
                var topDocs = PerformSearch(query, searcher, ref queryText, 1);
                if (topDocs.ScoreDocs.Length > 0)
                {
                    return GetResults(queryText, searcher, topDocs.ScoreDocs[0].Doc);
                }
                return new List<ResultItem>(); ;
            }
        }

        void IndexFile(string file)
        {
            var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
            using (var writer = new IndexWriter(ramDirectory, indexConfig))
            {
                string content = TextExtractor.ReadText(file);
                var doc = new Document
                {
                    new TextField("Content", content, Field.Store.YES),
                    new StringField("Path", file, Field.Store.YES)
                };

                writer.AddDocument(doc);
                writer.Flush(triggerMerge: true, applyAllDeletes: true);
            }
        }

        List <ResultItem> GetResults(string queryText, IndexSearcher searcher, int scoreDocId)
        {
            var path = searcher.Doc(scoreDocId).Get("Path");
            var snippets = Fragmentor.GetFragments(searcher, scoreDocId, parser.ParseSpanQuery(queryText, slop), analyzer);

            List<ResultItem> results = new List<ResultItem>();
            foreach (var snippet in snippets)
            {
                results.Add(new ResultItem
                {
                    TreeNode = new Tree.TreeNode(path),
                    Snippet = snippet
                });
            }
            return results;
        }
    }
}

