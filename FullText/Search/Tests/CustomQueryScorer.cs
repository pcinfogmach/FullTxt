using J2N.Text;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Search.Tests
{
    internal class CustomQueryScorer : IScorer
    {
        private float totalScore;

        private ISet<string> foundTerms;

        private IDictionary<string, WeightedSpanTerm> fieldWeightedSpanTerms;

        private readonly float maxTermWeight;

        private int position = -1;

        private readonly string defaultField;

        private ICharTermAttribute termAtt;

        private IPositionIncrementAttribute posIncAtt;

        private bool expandMultiTermQuery = true;

        private Query query;

        private string field;

        private IndexReader reader;

        private readonly bool skipInitExtractor;

        private bool wrapToCaching = true;

        private int maxCharsToAnalyze;

        public virtual float FragmentScore => totalScore;

        //
        // Summary:
        //     The highest weighted term (useful for passing to Lucene.Net.Search.Highlight.GradientFormatter
        //     to set top end of coloring scale).
        public virtual float MaxTermWeight => maxTermWeight;

        //
        // Summary:
        //     Controls whether or not multi-term queries are expanded against a Lucene.Net.Index.Memory.MemoryIndex
        //     Lucene.Net.Index.IndexReader. true if multi-term queries should be expanded
        public virtual bool ExpandMultiTermQuery
        {
            get
            {
                return expandMultiTermQuery;
            }
            set
            {
                expandMultiTermQuery = value;
            }
        }

        //
        // Summary:
        //     Constructs a new Lucene.Net.Search.Highlight.QueryScorer instance
        //
        // Parameters:
        //   query:
        //     Lucene.Net.Search.Query to use for highlighting
        public CustomQueryScorer(Query query)
        {
            Init(query, null, null, expandMultiTermQuery: true);
        }

        //
        // Summary:
        //     Constructs a new Lucene.Net.Search.Highlight.QueryScorer instance
        //
        // Parameters:
        //   query:
        //     Lucene.Net.Search.Query to use for highlighting
        //
        //   field:
        //     Field to highlight - pass null to ignore fields
        public CustomQueryScorer(Query query, string field)
        {
            Init(query, field, null, expandMultiTermQuery: true);
        }

        //
        // Summary:
        //     Constructs a new Lucene.Net.Search.Highlight.QueryScorer instance
        //
        // Parameters:
        //   query:
        //     Lucene.Net.Search.Query to use for highlighting
        //
        //   reader:
        //     Lucene.Net.Index.IndexReader to use for quasi tf/idf scoring
        //
        //   field:
        //     Field to highlight - pass null to ignore fields
        public CustomQueryScorer(Query query, IndexReader reader, string field)
        {
            Init(query, field, reader, expandMultiTermQuery: true);
        }

        //
        // Summary:
        //     Constructs a new Lucene.Net.Search.Highlight.QueryScorer instance
        //
        // Parameters:
        //   query:
        //     Lucene.Net.Search.Query to use for highlighting
        //
        //   reader:
        //     Lucene.Net.Index.IndexReader to use for quasi tf/idf scoring
        //
        //   field:
        //     Field to highlight - pass null to ignore fields
        //
        //   defaultField:
        //     The default field for queries with the field name unspecified
        public CustomQueryScorer(Query query, IndexReader reader, string field, string defaultField)
        {
            this.defaultField = defaultField.Intern();
            Init(query, field, reader, expandMultiTermQuery: true);
        }

        //
        // Summary:
        //     Constructs a new Lucene.Net.Search.Highlight.QueryScorer instance
        //
        // Parameters:
        //   query:
        //     Lucene.Net.Search.Query to use for highlighting
        //
        //   field:
        //     Field to highlight - pass null to ignore fields
        //
        //   defaultField:
        //     The default field for queries with the field name unspecified
        public CustomQueryScorer(Query query, string field, string defaultField)
        {
            this.defaultField = defaultField.Intern();
            Init(query, field, null, expandMultiTermQuery: true);
        }

        //
        // Summary:
        //     Constructs a new Lucene.Net.Search.Highlight.QueryScorer instance
        //
        // Parameters:
        //   weightedTerms:
        //     an array of pre-created Lucene.Net.Search.Highlight.WeightedSpanTerms
        public CustomQueryScorer(WeightedSpanTerm[] weightedTerms)
        {
            fieldWeightedSpanTerms = new J2N.Collections.Generic.Dictionary<string, WeightedSpanTerm>(weightedTerms.Length);
            foreach (WeightedSpanTerm weightedSpanTerm in weightedTerms)
            {
                if (!fieldWeightedSpanTerms.TryGetValue(weightedSpanTerm.Term, out var value) || value == null || value.Weight < weightedSpanTerm.Weight)
                {
                    fieldWeightedSpanTerms[weightedSpanTerm.Term] = weightedSpanTerm;
                    maxTermWeight = Math.Max(maxTermWeight, weightedSpanTerm.Weight);
                }
            }

            skipInitExtractor = true;
        }

        public virtual float GetTokenScore()
        {
            position += posIncAtt.PositionIncrement;
            string text = termAtt.ToString();
            if (!fieldWeightedSpanTerms.TryGetValue(text, out var value) || value == null)
            {
                return 0f;
            }

            if (value.IsPositionSensitive && !value.CheckPosition(position))
            {
                return 0f;
            }

            float weight = value.Weight;
            if (!foundTerms.Contains(text))
            {
                totalScore += weight;
                foundTerms.Add(text);
            }

            return weight;
        }

        public virtual TokenStream Init(TokenStream tokenStream)
        {
            position = -1;
            termAtt = tokenStream.AddAttribute<ICharTermAttribute>();
            posIncAtt = tokenStream.AddAttribute<IPositionIncrementAttribute>();
            if (!skipInitExtractor)
            {
                fieldWeightedSpanTerms?.Clear();
                return InitExtractor(tokenStream);
            }

            return null;
        }

        //
        // Summary:
        //     Retrieve the Lucene.Net.Search.Highlight.WeightedSpanTerm for the specified token.
        //     Useful for passing Span information to a Lucene.Net.Search.Highlight.IFragmenter.
        //
        //
        // Parameters:
        //   token:
        //     token to get Lucene.Net.Search.Highlight.WeightedSpanTerm for
        //
        // Returns:
        //     Lucene.Net.Search.Highlight.WeightedSpanTerm for token
        public virtual WeightedSpanTerm GetWeightedSpanTerm(string token)
        {
            fieldWeightedSpanTerms.TryGetValue(token, out var value);
            return value;
        }

        private void Init(Query query, string field, IndexReader reader, bool expandMultiTermQuery)
        {
            this.reader = reader;
            this.expandMultiTermQuery = expandMultiTermQuery;
            this.query = query;
            this.field = field;
        }

        private TokenStream InitExtractor(TokenStream tokenStream)
        {
            WeightedSpanTermExtractor weightedSpanTermExtractor = NewTermExtractor(defaultField);
            //weightedSpanTermExtractor.SetMaxDocCharsToAnalyze(maxCharsToAnalyze);
            weightedSpanTermExtractor.ExpandMultiTermQuery = expandMultiTermQuery;
            weightedSpanTermExtractor.SetWrapIfNotCachingTokenFilter(wrapToCaching);
            if (reader == null)
            {
                fieldWeightedSpanTerms = weightedSpanTermExtractor.GetWeightedSpanTerms(query, tokenStream, field);
            }
            else
            {
                fieldWeightedSpanTerms = weightedSpanTermExtractor.GetWeightedSpanTermsWithScores(query, tokenStream, field, reader);
            }

            if (weightedSpanTermExtractor.IsCachedTokenStream)
            {
                return weightedSpanTermExtractor.TokenStream;
            }

            return null;
        }

        protected virtual WeightedSpanTermExtractor NewTermExtractor(string defaultField)
        {
            if (defaultField != null)
            {
                return new WeightedSpanTermExtractor(defaultField);
            }

            return new WeightedSpanTermExtractor();
        }

        public virtual void StartFragment(TextFragment newFragment)
        {
            foundTerms = new J2N.Collections.Generic.HashSet<string>();
            totalScore = 0f;
        }

        //
        // Summary:
        //     By default, Lucene.Net.Analysis.TokenStreams that are not of the type Lucene.Net.Analysis.CachingTokenFilter
        //     are wrapped in a Lucene.Net.Analysis.CachingTokenFilter to ensure an efficient
        //     reset - if you are already using a different caching Lucene.Net.Analysis.TokenStream
        //     impl and you don't want it to be wrapped, set this to false.
        public virtual void SetWrapIfNotCachingTokenFilter(bool wrap)
        {
            wrapToCaching = wrap;
        }

        public virtual void SetMaxDocCharsToAnalyze(int maxDocCharsToAnalyze)
        {
            maxCharsToAnalyze = maxDocCharsToAnalyze;
        }
    }
}

