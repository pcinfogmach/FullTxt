using Lucene.Net.Analysis;
using Lucene.Net.Util;
using System;

namespace FullText.Search
{
    public class LuceneBase
    {
        public string indexPath;
        public Analyzer analyzer;
        public HebrewQueryParser parser;
        public LuceneBase()
        {
            indexPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Index");
            if (!System.IO.Directory.Exists(indexPath)) { System.IO.Directory.CreateDirectory(indexPath); }
            analyzer = new HebrewAnalyzer(LuceneVersion.LUCENE_48);
            parser = new HebrewQueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Content", analyzer);
        }
    }
}
