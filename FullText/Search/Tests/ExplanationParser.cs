using Lucene.Net.Index;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FullText.Search.Tests
{
    public static class ExplanationParser
    {
        public static List<string> ParseExplanation(IndexSearcher searcher, Query query, int docId)
        {
            //var termsVector = searcher.IndexReader.GetTermVector(docId, "Content");
            //GetTermVectors(termsVector);

            GetHitTermsForDoc(query, searcher, docId);
            var terms = new List<string>();
            var explanation = searcher.Explain(query, docId);
            AnalyzeExplanation(explanation, ref terms);
            return terms;

            
        }

        private static void AnalyzeExplanation(Explanation explanation, ref List<string> terms)
        {
            if (explanation == null)
            {
                return;
            }

            if (explanation.IsMatch)
            {
                terms.Add(explanation.ToHtml());
                if (!string.IsNullOrEmpty(explanation.Description))
                {
                    terms.Add(explanation.Description);
                    var termsMatched = new List<List<string>>();
                    var splitString = explanation.Description.Split(new string[] { "])," }, StringSplitOptions.None);

                    foreach (string split in splitString)
                    {
                        MatchCollection matchCollection = Regex.Matches(split, @":([^ ]+)\^");

                        var matchList = new List<string>();

                        foreach (Match match in matchCollection)
                        {
                            matchList.Add(match.Groups[1].Value.Trim('_', ' '));
                        }

                        termsMatched.Add(matchList);
                    }
                    Console.Write("");
                }

                var details = explanation.GetDetails();
                if (details != null)
                {
                    foreach (var detail in details)
                    {
                        AnalyzeExplanation(detail, ref terms);
                    }
                }
            }
        }

        private static IEnumerable<string> GetHitTermsForDoc(Query query, IndexSearcher searcher, int docId)
        {
            var simplifiedQuery = query.Rewrite(searcher.IndexReader);
            HashSet<Term> queryTerms = new HashSet<Term>();
            simplifiedQuery.ExtractTerms(queryTerms);

            HashSet<string> hitTerms = new HashSet<string>();
            foreach (var term in queryTerms)
            {
                var termQuery = new TermQuery(term);
                var explanation = searcher.Explain(termQuery, docId);
                if (explanation.IsMatch)
                {
                    hitTerms.Add(explanation.ToHtml());
                }
            }
            return hitTerms;
        }

        static void GetTermVectors(Terms termsVector)
        {
            var enumerator = termsVector.GetEnumerator();
            var pos = enumerator.DocsAndPositions(null, null);
            return;
        }
    }

    public class MatchItem
    {
        public string Term { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

}
