using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FullTxtIndexer
{
    public static class HebrewConsole
    {
        static bool isHebrewEncoding;
        public static void WriteLine(string text)
        {
            if (!isHebrewEncoding) { Console.OutputEncoding = Encoding.GetEncoding("Windows-1255");  isHebrewEncoding = true; } 
            Console.WriteLine(text.ReverseHebrewCharacters());
        }
    }

    public static class HebrewStringFixer
    {
        public static string ReverseHebrewCharacters(this string input)
        {
            Regex hebrewRegex = new Regex(@"\p{IsHebrew}+(\W+\p{IsHebrew}+)?");
            return hebrewRegex.Replace(input, match => new string(match.Value.Reverse().ToArray()));
        }
    }
}
