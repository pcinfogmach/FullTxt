using FullText.Helpers;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

internal class CustomHighlighter
{
    private readonly QueryScorer _scorer;
    private readonly Query _query;
    private readonly Analyzer _analyzer;
    private readonly IndexSearcher _searcher;
    private readonly int _docId;
    private readonly HashSet<string> _hitTerms;

    public CustomHighlighter(Query query, Analyzer analyzer, IndexSearcher searcher, int docId)
    {
        _scorer = new QueryScorer(query);
        _query = query;
        _analyzer = analyzer;
        _searcher = searcher;
        _docId = docId;
        _hitTerms = new HashSet<string>(GetHitTermsForDoc(query, searcher, docId));
    }

    public List<string> HighlightText(string fieldName, int fragmentLength)
    {
        fragmentLength += _hitTerms.Sum(term => term.Length);
        string text = _searcher.Doc(_docId).Get(fieldName);
        return FindAllIndexes.CreateHighLightedSnippets(text, _hitTerms.ToArray(), 10);
    }   

    private IEnumerable<string> GetHitTermsForDoc(Query query, IndexSearcher searcher, int docId)
    {
        //var explainedSearch = searcher.Explain(query, docId);
        //var hitTermsexplained = GetHitTermAssociations(explainedSearch.Description, "ברכת המזון~1");
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
                hitTerms.Add(term.Text);
            }
        }
        return hitTerms;
    }

    
}
