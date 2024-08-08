using FullText.Controls;
using FullText.Helpers;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Index.Extensions;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.WindowsAPICodePack.Dialogs;
using sun.swing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Search
{
    public class LuceneIndexer : LuceneBase
    {
        public Regex idRegex = new Regex(@"\W", RegexOptions.Compiled);
        Queue<string> indexQueue = new Queue<string>();
        bool isIndexerRunning;

        public void CreateNewIndexingTask()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            string fileName = dialog.FileName;

            if (Properties.Settings.Default.RootFolderNodes.Contains(fileName))
            {
                var result = MessageBox.Show("תיקייה זו כבר קיימת באינדקס האם ברצונך לעדכן אותה?", "תיקייה קיימת", MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.Yes, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                if (result == MessageBoxResult.Yes) { Properties.Settings.Default.FolderNodeToChange = fileName; }
                else { return; }
            }

            indexQueue.Enqueue(fileName);
            if (!isIndexerRunning)
            {
                isIndexerRunning = true;
                ProcessQueue();
            }
        }

        private async void ProcessQueue()
        {
            while (indexQueue.Count > 0)
            {
                string directory = indexQueue.Dequeue();
                Dictionary<string, bool> files = System.IO.Directory.GetFiles(directory, "*.*", System.IO.SearchOption.AllDirectories).ToDictionary(f => f, s => false);
                await IndexFiles(files, directory);
            }
            isIndexerRunning = false;
        }

        public async Task IndexFiles(Dictionary<string, bool> filesToIndex, string directoryToIndex)
        {
            var files = filesToIndex.Select(file => file.Key).ToList();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var progressDialog = ProgressDialog.Start($"מאנדקס את: {directoryToIndex}", files.Count, cancellationTokenSource);
            Application.Current.MainWindow.Closing += (s, e) => { cancellationTokenSource.Cancel(); progressDialog.Report(-1); };


          

            //    await Task.Run(() =>
            //    {
            //        var helper = new LuceneIndexerHelper();
            //        try
            //        {
            //            Parallel.ForEach(files, new ParallelOptions {  CancellationToken = cancellationTokenSource.Token }, (file) =>
            //            {
            //                Console.WriteLine(file);

            //                string content = TextExtractor.ReadText(file);
            //                string id = idRegex.Replace(file, "");

            //                lock (helper)
            //                {
            //                     helper.IndexFileAsync(file, id, content).Wait();
            //                }

            //                filesToIndex[file] = true;
            //                progressDialog.Report(1);

            //            });
            //        }
            //        catch (Exception ex) { MessageBox.Show(ex.Message); }
            //    });
            

            //using (IndexWriter IndexWriter = new IndexWriter(FSDirectory.Open(new DirectoryInfo(indexPath)),
            //    new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)))
            //{
            //    if (!cancellationTokenSource.Token.IsCancellationRequested) IndexWriter.Commit();
            //}

            await Task.Delay(100);
            if (!cancellationTokenSource.Token.IsCancellationRequested)
                Properties.Settings.Default.FolderNodeToChange = directoryToIndex;
            progressDialog.Report(-1);
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


//async Task IndexFile(string filePath, IndexWriter writer)
//{
//    const long TwoMB = 2 * 1024 * 1024;

//    var fileInfo = new FileInfo(filePath);
//    if (fileInfo.Length > TwoMB)
//    {
//        Task.Run(async () =>
//        {
//            await IndexFileAsync(filePath, writer);
//            writer.Flush(triggerMerge: true, applyAllDeletes: true);
//        });
//    }
//    else
//    {
//        await IndexFileAsync(filePath, writer);
//    }
//}

//Application.Current.Dispatcher.Invoke(() => { Application.Current.Exit += (s, e) => { SaveInterruptedIndexing(filesToIndex, writer); }; });

//    var tasks = files.Select(async (file) =>
//    {
//        if (string.IsNullOrEmpty(file) || cancellationTokenSource.Token.IsCancellationRequested) { return; }
//        string id = idRegex.Replace(file, "");
//        string content = TextExtractor.ReadText(file);
//        var doc = new Document
//{
//    new StringField("Path", file, Field.Store.YES),
//    new StringField("Id", id, Field.Store.YES),
//    new TextField("Content", content, Field.Store.YES)
//};
//        lock (writer)
//        {
//            writer.UpdateDocument(new Term("Id", id), doc);  // Update the document if it exists, otherwise add it
//        }

//        filesToIndex[file] = true;
//        progressDialog.Report(1);
//    });

//    await Task.WhenAll(tasks);

//public  LuceneIndexer()
//{
//    //Application.Current.LoadCompleted += Current_LoadCompleted;
//}

//async void Current_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
//{
//    if (Properties.Settings.Default.IndexerPendingFiles.Count > 0)
//    {
//        Dictionary<string, bool> files = Properties.Settings.Default.IndexerPendingFiles.Cast<string>().ToDictionary(f => f, s => false);
//        await IndexFilesAsync(files, string.Empty);
//        Properties.Settings.Default.IndexerPendingFiles.Clear();
//        foreach (string folder in Properties.Settings.Default.IndexerPendingFolders)
//        {
//            Properties.Settings.Default.FolderNodeToChange = folder;
//        }
//        Properties.Settings.Default.IndexerPendingFolders.Clear();
//        Properties.Settings.Default.Save();
//    }
//}

//void SaveInterruptedIndexing(Dictionary<string, bool> filesToIndex, IndexWriter writer)
//{
//    Properties.Settings.Default.IndexerPendingFiles.AddRange(filesToIndex.Where(file => file.Value == false).Select(file => file.Key).ToArray());
//    Properties.Settings.Default.Save();
//    writer.Flush(triggerMerge: true, applyAllDeletes: true);
//}
