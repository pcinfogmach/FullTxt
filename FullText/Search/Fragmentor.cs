using Lucene.Net.Search.Highlight;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;

namespace FullText.Search
{
    public static class Fragmentor
    {
        public static string[] GetFragments(IndexSearcher searcher, int docId, Query query, Analyzer analyzer)
        {
            var reader = searcher.IndexReader;
            var scorer = new QueryScorer(query);
            var fragmenter = new SimpleSpanFragmenter(scorer, 150); // the int is the fragment size 

            var formatter = new SimpleHTMLFormatter("<mark>", "</mark>"); //define how to mark found keywords if left empty <b> tags is the default
            var highlighter = new Highlighter(formatter, scorer);
            highlighter.TextFragmenter = fragmenter;

            var content = searcher.Doc(docId).Get("Content");
            var tokenStream = TokenSources.GetAnyTokenStream(reader, docId, "Content", analyzer);
            var fragments = highlighter.GetBestTextFragments(tokenStream, content, false, 30); // 10 is the number of snippets

            return fragments.Where(fragment => fragment.Score > 0).Select(fragment => fragment.ToString()).ToList().ToArray();
        }
    }
}
