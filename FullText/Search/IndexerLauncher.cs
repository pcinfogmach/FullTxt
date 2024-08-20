using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FullText.Search
{
    internal class IndexerLauncher
    {
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

        private void ProcessQueue()
        {
            while (indexQueue.Count > 0)
            {
                string directory = indexQueue.Dequeue();
                List<string> files = System.IO.Directory.GetFiles(directory, "*.*", System.IO.SearchOption.AllDirectories).ToList();
                IndexFiles(files, directory);
            }
            isIndexerRunning = false;
        }

        void IndexFiles(List<string> files, string directory)
        {
            Task.Run(() =>
            {
                try
                {
                    using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("FullTxtIndexerPipe", PipeDirection.InOut))
                    {
                        pipeServer.WaitForConnection();

                        using (StreamReader reader = new StreamReader(pipeServer))
                        using (StreamWriter writer = new StreamWriter(pipeServer) { AutoFlush = true })
                        {
                            for (int i = 0; i < files.Count; i += 100)
                            {
                                var chunk = files.Skip(i).Take(100).ToList();
                                foreach (var filePath in chunk)
                                {
                                    //if (reader.ReadLine() == "CANCELED")
                                    //    break;
                                    writer.WriteLine(filePath);
                                }
                                //if (reader.ReadLine() == "CANCELED") break;
                                writer.WriteLine("END_OF_CHUNK");
                            }
                            writer.WriteLine("END_OF_LIST");
                        }
                    }
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            });
        }
            

    }
}
