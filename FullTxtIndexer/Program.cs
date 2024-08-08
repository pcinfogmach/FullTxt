using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FullText.Search;

namespace FullTxtIndexer
{
    internal class Program
    {
        static List<string> indexingQueue = new List<string>();
        static void Main(string[] args)
        {
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", "FullTxtIndexerPipe", PipeDirection.InOut))
            {
                Console.WriteLine("Connecting to server...\n");
                pipeClient.Connect();

                StreamReader reader = new StreamReader(pipeClient);
                StreamWriter writer = new StreamWriter(pipeClient) { AutoFlush = true };

                Console.CancelKeyPress += (s, e) => { writer.WriteLine("CANCELED"); };
                AppDomain.CurrentDomain.ProcessExit += (s, e) => { writer.WriteLine("CANCELED"); };

                string message;
                while ((message = reader.ReadLine()) != "END_OF_LIST")
                {
                    if (message == "END_OF_CHUNK") 
                    {
                        new LuceneIndexer().IndexFiles(indexingQueue);
                        indexingQueue.Clear();
                    }
                    else indexingQueue.Add(message);
                }
                writer.WriteLine("done");
            }
        }
    }
}
