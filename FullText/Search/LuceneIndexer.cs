using FullText.Controls;
using FullText.Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Search
{
    public class LuceneIndexer : LuceneBase
    {
        public Regex idRegex = new Regex(@"\W", RegexOptions.Compiled);
        Queue<string> indexQueue = new Queue<string>();
        bool isIndexerRunning;

        public async Task NewIndexingTask()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            CommonFileDialogResult dialogResult = CommonFileDialogResult.None;
            Application.Current.Dispatcher.Invoke(() => { dialogResult = dialog.ShowDialog();});
            if (dialogResult != CommonFileDialogResult.Ok) { return; }

            if (Properties.Settings.Default.RootFolderNodes.Contains(dialog.FileName))
            {
                var result = MessageBox.Show("תיקייה זו כבר קיימת באינדקס האם ברצונך לעדכן אותה?", "תיקייה קיימת", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                if (result == MessageBoxResult.Yes) { Properties.Settings.Default.NodeToChange = dialog.FileName; }
                else { return; }
            }

            indexQueue.Enqueue(dialog.FileName);
            if (!isIndexerRunning)
            {
                isIndexerRunning = true;
                await ProcessQueueAsync();
                isIndexerRunning = false;
            }
        }


        private async Task ProcessQueueAsync()
        {
            while (indexQueue.Count > 0)
            {
                await IndexFilesAsync(indexQueue.Dequeue());
            }
        }

        public async Task IndexFilesAsync(string directoryToIndex)
        {
            var files = System.IO.Directory.GetFiles(directoryToIndex, "*.*", System.IO.SearchOption.AllDirectories).ToList();
            var progressReporter = ProgressDialog.Start($"מאנדקס את: {directoryToIndex}", files.Count);
            using (var directory = FSDirectory.Open(new DirectoryInfo(indexPath)))
            {
                var indexConfig = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
                using (var writer = new IndexWriter(directory, indexConfig))
                {
                    var tasks = files.Select(async (file) =>
                    {
                        string id = idRegex.Replace(file, "");
                        string content = TextExtractor.ReadText(file);
                        var doc = new Document
                {
                    new StringField("Path", file, Field.Store.YES),
                    new StringField("Id", id, Field.Store.YES),
                    new TextField("Content", content, Field.Store.YES)
                };
                        lock (writer)
                        {
                            writer.UpdateDocument(new Term("Id", id), doc);  // Update the document if it exists, otherwise add it
                        }

                        progressReporter.Report(1);
                    });

                    await Task.WhenAll(tasks);
                    writer.Flush(triggerMerge: true, applyAllDeletes: true);
                }
            }

            Properties.Settings.Default.NodeToChange = directoryToIndex;
            progressReporter.Report(-1);
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
