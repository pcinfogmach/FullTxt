using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace SynynomsFetcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");


            string word = "אא\"כ";
            var synynoms = new SynynomsFetcher().MsWordSynynoms(word);
            foreach (var synynoms_syn in synynoms) 
            {
                Console.WriteLine(new string(synynoms_syn.Reverse().ToArray()));
            }

            // Keep the console open until the user presses Enter
            Console.WriteLine("Press Enter to close the console...");
            Console.ReadLine();
        }

        static void WriteLineRtl(string input)
        {
            CursorLeft = WindowWidth - input.Length;
            for (int i = input.Length - 1; i >= 0; i--)
                Write(input[i]);
        }
    }
}
