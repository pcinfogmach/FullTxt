using Lucene.Net.Analysis;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FullText.Search
{
    public class HtmlStrippingCharFilter : CharFilter
    {
        private readonly TextReader input;
        private readonly string strippedText;
        private int currentPos;
        private int cumulativeDiff;

        public HtmlStrippingCharFilter(TextReader input) : base(input)
        {
            this.input = input;
            this.strippedText = StripHtmlTags(ReadAll(input));
            this.currentPos = 0;
            this.cumulativeDiff = 0;
        }

        private static string ReadAll(TextReader reader)
        {
            return reader.ReadToEnd();
        }

        public override int Read(char[] buffer, int offset, int length)
        {
            if (currentPos >= strippedText.Length) return -1;

            int charsToRead = Math.Min(length, strippedText.Length - currentPos);
            strippedText.CopyTo(currentPos, buffer, offset, charsToRead);
            currentPos += charsToRead;
            return charsToRead;
        }

        public override int Read()
        {
            if (currentPos >= strippedText.Length) return -1;
            return strippedText[currentPos++];
        }

        private static string StripHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        protected override int Correct(int currentOff)
        {
            return currentOff + cumulativeDiff;
        }
    }
}
