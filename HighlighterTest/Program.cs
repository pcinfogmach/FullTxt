using HighlighterTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FullText.Search.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");

            // Sample text to search in
            var text = SmapleText.MalbimHaggadaPdf();

            // Boolean query structure (synonyms in inner lists as ReadOnlyMemory<char>)
            var booleanQueryStructure = new List<List<ReadOnlyMemory<char>>>
            {
                new List<ReadOnlyMemory<char>> { "מותר".AsMemory(), "אסור".AsMemory() },    
            };

            // Benchmarking
            const int iterations = 1;
            Stopwatch stopwatch = new Stopwatch();

            List<string> snippets = new List<string>();
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                snippets = CustomHtmlHighlighter4.FindAndHighlightAllBooleanMatches(text.AsSpan(), booleanQueryStructure);
            }
            stopwatch.Stop();

            //Console.WriteLine("Original Text:");
            //Console.WriteLine(text);

            foreach (var snippet in snippets)
            {
                Console.WriteLine("\nsnippet:");
                Console.WriteLine(snippet.ReverseHebrewCharacters());
            }
           

            // Output benchmark results
            Console.WriteLine($"\nExecuted {iterations} iterations in {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Average time per iteration: {stopwatch.ElapsedMilliseconds / (double)iterations} ms");

            // Keep the console window open
            Console.ReadLine();
        }
    }

    public static class HebrewStringFixer
    {
        public static string ReverseHebrewCharacters(this string input)
        {
            Regex hebrewRegex = new Regex(@"\p{IsHebrew}+|\p{IsHebrew}+\W|\p{IsHebrew}+\W+\p{IsHebrew}+");
            return hebrewRegex.Replace(input, match => new string(match.Value.Reverse().ToArray()));
        }
    }
}
