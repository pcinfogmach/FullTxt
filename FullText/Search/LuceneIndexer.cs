using FullText.Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Search
{
    public  class LuceneIndexer
    {
        public string indexPath;
        public Analyzer analyzer;
        public HebrewQueryParser parser;
        public Regex idRegex = new Regex(@"\W", RegexOptions.Compiled);

        public LuceneIndexer()
        {
            indexPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LuceneFullTextIndex");
            if (!System.IO.Directory.Exists(indexPath)) { System.IO.Directory.CreateDirectory(indexPath); }
            analyzer = new HebrewAnalyzer(LuceneVersion.LUCENE_48);
            parser = new HebrewQueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, "Content", analyzer);
        }

        public void IndexFiles(List<string> files)
        {
            using (var directory = FSDirectory.Open(new DirectoryInfo(indexPath)))
            {
                var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
                using (var writer = new IndexWriter(directory, indexConfig))
                {
                    Parallel.ForEach(files, (file) =>
                    {
                        string id = idRegex.Replace(file, "");
                        string content = TextExtractor.ReadText(file).RemoveEmptyLines();
                        var doc = new Document
                        {
                            new StringField("Path", file, Field.Store.YES),
                            new StringField("Id", id, Field.Store.YES),
                            new TextField("Content", content, Field.Store.YES)
                        };
                        writer.UpdateDocument(new Term("Id", id), doc);  // Update the document if it exists, otherwise add it
                    });
                    writer.Flush(triggerMerge: true, applyAllDeletes: true);
                }
            }
        }

        public void RemoveFiles(List<string> files)
        {
            try
            {
                using (var directory = FSDirectory.Open(new DirectoryInfo(indexPath)))
                {
                    var indexConfig = new IndexWriterConfig(Lucene.Net.Util.LuceneVersion.LUCENE_48, analyzer);
                    using (var writer = new IndexWriter(directory, indexConfig))
                    {
                        var parser = new SimpleQueryParser(analyzer, "Id");
                        Parallel.ForEach(files, (file) =>
                        {
                            string id = idRegex.Replace(file, "");
                            Query query = parser.Parse(id);
                            writer.DeleteDocuments(query);
                        });
                        writer.Flush(triggerMerge: false, applyAllDeletes: false);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
