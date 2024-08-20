using Lucene.Net.Search.Highlight;
using Lucene.Net.Search;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Search.Spans;
using FullText.Search.Tests;
using System.Collections.Generic;

namespace FullText.Search
{
    public static class Fragmentor
    {
        public static List<string> GetFragments(IndexSearcher searcher, int docId, Query query, Analyzer analyzer)
        {
            var reader = searcher.IndexReader;
            var scorer = new QueryScorer(query);
            var fragmenter = new SimpleSpanFragmenter(scorer, 150); // the int is the fragment size 

            var formatter = new SimpleHTMLFormatter("<mark>", "</mark>"); //define how to mark found keywords if left empty <b> tags is the default
            var highlighter = new Highlighter(formatter, scorer);
            highlighter.TextFragmenter = fragmenter;

            var content = searcher.Doc(docId).Get("Content");
            var tokenStream = TokenSources.GetAnyTokenStream(reader, docId, "Content", analyzer);
            var fragments = highlighter.GetBestTextFragments(tokenStream, content, false, short.MaxValue);
            return fragments.Where(fragment => fragment.Score > 0).Select(fragment => fragment.ToString()).ToList();

            //var highlighter = new CustomHighlighter(query, analyzer, searcher, docId);
            //List<string> fragments = highlighter.HighlightText("Content", 150);


            //return fragments;
        }
    }
}
