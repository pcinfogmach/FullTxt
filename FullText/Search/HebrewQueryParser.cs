using Lucene.Net.QueryParsers.ComplexPhrase;
using Lucene.Net.Search.Spans;
using Lucene.Net.Search;
using Lucene.Net.Util;
using org.apache.sis.metadata.iso.acquisition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers.Classic;
using System.Text.RegularExpressions;
using Lucene.Net.Analysis;

namespace FullText.Search
{
    public class HebrewQueryParser : ComplexPhraseQueryParser
    {
        public HebrewQueryParser(LuceneVersion luceneVersion, string field, Analyzer analyzer)
            : base(luceneVersion, field, analyzer)
        {
            AllowLeadingWildcard = true;
            DefaultOperator = Operator.AND;
        }

        public override Query Parse(string queryText)
        {
            if (string.IsNullOrEmpty(queryText)) { return base.Parse("parsing error placeholder"); }
            try
            {
                queryText = queryText.Trim().Trim('|');
                queryText = queryText.Replace("|", "OR");
                queryText = queryText.TrimStart('?');
                queryText = Regex.Replace(queryText, @"([\w]*?)(.)(\?)([\w""']*)", @"$1$2$4 OR $1$4");
                string escapedQueryText = EscapeDoubleQuotes(queryText);
                return base.Parse(escapedQueryText);
            }
            catch
            {
                return new Lucene.Net.QueryParsers.Simple.SimpleQueryParser(Analyzer, Field).Parse(queryText);
            }
        }



        public SpanNearQuery ParseSpanQuery(string queryText, int distanceBetweenWords)
        {
            var parsedQuery = Parse(queryText);
            var spanQuery = SpanQueryParser(parsedQuery, distanceBetweenWords);
            return new SpanNearQuery(spanQuery, distanceBetweenWords, false); ;
        }

        SpanQuery[] SpanQueryParser(Query parsedQuery, int distanceBetweenWords)
        {
            List<SpanQuery> spanQueries = new List<SpanQuery>();
            if (parsedQuery is BooleanQuery booleanQuery)
            {
                List<SpanQuery> occurShouldClauses = new List<SpanQuery>();
                foreach (BooleanClause clause in booleanQuery.Clauses)
                {
                    Query subQuery = clause.Query;
                    if (clause.Occur == Occur.SHOULD)
                    {
                        occurShouldClauses.Add(ConvertedQuery(subQuery, distanceBetweenWords));
                    }
                    else
                    {
                        if (occurShouldClauses.Count > 1)
                        {
                            spanQueries.Add(new SpanOrQuery(occurShouldClauses.ToArray()));
                            occurShouldClauses = new List<SpanQuery>();
                        }
                        spanQueries.Add(ConvertedQuery(subQuery, distanceBetweenWords));
                    }
                }

                if (occurShouldClauses.Count > 1)
                {
                    spanQueries.Add(new SpanOrQuery(occurShouldClauses.ToArray()));
                    occurShouldClauses.Clear();
                }
            }
            else
            {
                spanQueries.Add(ConvertedQuery(parsedQuery, distanceBetweenWords));
            }

            return spanQueries.ToArray();
        }

        SpanQuery ConvertedQuery(Query query, int distanceBetweenWords)
        {
            if (query is BooleanQuery booleanQuery)
            {
                SpanQuery[] spanQueries = SpanQueryParser(booleanQuery, distanceBetweenWords);
                return new SpanNearQuery(spanQueries, distanceBetweenWords, false);
            }
            else if (query is TermQuery termQuery)
            {
                return new SpanTermQuery(termQuery.Term);
            }
            else if (query is WildcardQuery wildcardQuery)
            {
                return new SpanMultiTermQueryWrapper<WildcardQuery>(wildcardQuery);
            }
            else if (query is FuzzyQuery fuzzyQuery)
            {
                return new SpanMultiTermQueryWrapper<FuzzyQuery>(fuzzyQuery);
            }
            return null;
        }

        private string EscapeDoubleQuotes(string input)
        {
            return Regex.Replace(input, "\"", "\\\"");
        }
    }
}
