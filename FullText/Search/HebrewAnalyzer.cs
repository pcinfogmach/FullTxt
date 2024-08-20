using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Analysis;
using Lucene.Net.Util;
using System.IO;
using System.Text.RegularExpressions;

namespace FullText.Search
{
    internal class HebrewAnalyzer : Analyzer
    {
        private readonly LuceneVersion version;

        public HebrewAnalyzer(LuceneVersion luceneVersion)
        {
            version = luceneVersion;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(version, reader);
            TokenStream filter = new HebrewTokenFilter(tokenizer);
            return new TokenStreamComponents(tokenizer, filter);
        }

        protected override TextReader InitReader(string fieldName, TextReader reader)
        {
            return new HtmlStrippingCharFilter(reader);
        }

        private sealed class HebrewTokenFilter : TokenFilter
        {
            private readonly ICharTermAttribute termAttr;
            Regex diacriticsRegex = new Regex(@"\p{M}");

            public HebrewTokenFilter(TokenStream input) : base(input)
            {
                termAttr = AddAttribute<ICharTermAttribute>();
            }

            public sealed override bool IncrementToken()
            {
                if (!m_input.IncrementToken())
                {
                    return false;
                }

                string token = termAttr.ToString();
                string cleanedToken = diacriticsRegex.Replace(token, ""); // Removing diacritics 

                if (!string.Equals(token, cleanedToken))
                {
                    termAttr.SetEmpty().Append(cleanedToken);
                }

                return true;
            }
        }
    }
}
