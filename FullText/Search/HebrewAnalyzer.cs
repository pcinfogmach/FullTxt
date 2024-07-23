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
        LuceneVersion version;
        public HebrewAnalyzer(LuceneVersion luceneVersion)
        {
            version = luceneVersion;
        }
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(version, reader);
            TokenStream filter = new HebrewTokenFilter(tokenizer);
            filter = new LowerCaseFilter(version, filter);
            filter = new StopFilter(version, filter, StopAnalyzer.ENGLISH_STOP_WORDS_SET);
            return new TokenStreamComponents(tokenizer, filter);
        }

        protected override TextReader InitReader(string fieldName, TextReader reader)
        {
            return new HtmlStrippingCharFilter(reader);
        }

        sealed class HebrewTokenFilter : TokenFilter
        {
            private readonly ICharTermAttribute termAttr;

            public HebrewTokenFilter(TokenStream input) : base(input)
            {
                this.termAttr = AddAttribute<ICharTermAttribute>();
            }

            public sealed override bool IncrementToken()
            {
                if (m_input.IncrementToken())
                {
                    string token = termAttr.ToString();
                    string cleanedToken = Regex.Replace(token, @"\p{M}", "");

                    if (!string.Equals(token, cleanedToken))
                    {
                        termAttr.SetEmpty().Append(cleanedToken);
                    }

                    return true;
                }
                return false;
            }
        }
    }
}
