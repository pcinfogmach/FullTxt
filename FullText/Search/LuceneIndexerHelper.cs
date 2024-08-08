using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Search
{
    internal class LuceneIndexerHelper : LuceneBase
    {
        public async Task IndexFileAsync(string filePath, string id, string content)
        {
            using (IndexWriter writer = new IndexWriter(FSDirectory.Open(new DirectoryInfo(indexPath)),
                new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)))
            {
                writer.UpdateDocument(new Term("Id", id), new Document // Update the document if it exists, otherwise add it
                        {
                            new StringField("Path", filePath, Field.Store.YES),
                            new StringField("Id", id, Field.Store.YES),
                            new TextField("Content", content, Field.Store.YES)
                        });
            }
        }
    }
}
