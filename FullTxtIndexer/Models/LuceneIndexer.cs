using FullText.Helpers;
using FullTxtIndexer;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FullText.Search
{
    public class LuceneIndexer
    {
        public string indexPath;
        public Analyzer analyzer;
        public Regex idRegex = new Regex(@"\W", RegexOptions.Compiled);

        public LuceneIndexer()
        {
            string parentDirectory = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            indexPath = System.IO.Path.Combine(parentDirectory, "Index");
            if (!System.IO.Directory.Exists(indexPath)) { System.IO.Directory.CreateDirectory(indexPath); }
            analyzer = new HebrewAnalyzer(LuceneVersion.LUCENE_48);
        }

        public void IndexFiles(List<string> files)
        {
            using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(indexPath)),
              new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)))
            {

                Parallel.ForEach(files, (file) =>
                {
                    try
                    {
                        HebrewConsole.WriteLine(file);

                        string content = TextExtractor.ReadText(file);
                        string id = idRegex.Replace(file, "");

                        lock (writer)
                        {
                            writer.UpdateDocument(new Term("Id", id), new Document // Update the document if it exists, otherwise add it
                            {
                            new StringField("Path", file, Field.Store.YES),
                            new StringField("Id", id, Field.Store.YES),
                            new TextField("Content", content, Field.Store.YES)
                            });
                        }
                    }
                    catch (Exception ex) { HebrewConsole.WriteLine(ex.Message); }
                });
            }
        }
    }
}

