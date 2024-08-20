using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal class CustomHighlighter1
{
    private readonly QueryScorer _scorer;
    private readonly Query _query;
    private readonly Analyzer _analyzer;
    private readonly IndexSearcher _searcher;
    private readonly int _docId;
    private readonly HashSet<string> _hitTerms;

    public CustomHighlighter1(Query query, Analyzer analyzer, IndexSearcher searcher, int docId)
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
        TokenStream tokenStream = _analyzer.GetTokenStream(fieldName, new System.IO.StringReader(text));
        IOffsetAttribute offsetAttribute = tokenStream.AddAttribute<IOffsetAttribute>();
        ICharTermAttribute charTermAttribute = tokenStream.AddAttribute<ICharTermAttribute>();

        List<string> snippets = new List<string>();
        StringBuilder currentSnippet = new StringBuilder();
        int currentOffset = 0;
        int snippetStartOffset = 0;

        // Track which hit terms are present in the current snippet
        HashSet<string> foundHitTerms = new HashSet<string>();

        tokenStream.Reset();

        while (tokenStream.IncrementToken())
        {
            int startOffset = offsetAttribute.StartOffset;
            int endOffset = offsetAttribute.EndOffset;
            string term = charTermAttribute.ToString();

            // Start a new snippet if the current one exceeds the fragment length
            if (currentSnippet.Length > 0 && startOffset - snippetStartOffset >= fragmentLength)
            {
                if (foundHitTerms.SetEquals(_hitTerms)) // Check if all hit terms are present
                {
                    snippets.Add(currentSnippet.ToString());
                }
                currentSnippet.Clear();
                snippetStartOffset = startOffset;
                foundHitTerms.Clear();
            }

            // Append text between tokens
            if (currentOffset < startOffset)
            {
                AppendTerm(currentSnippet, text.Substring(currentOffset, startOffset - currentOffset));
            }

            // Check if the term is a hit and highlight if necessary
            if (_hitTerms.Contains(term))
            {
                AppendTerm(currentSnippet, HighlightToken(term));
                foundHitTerms.Add(term);
            }
            else
            {
                AppendTerm(currentSnippet, term);
            }

            currentOffset = endOffset;
        }

        // Add the last snippet if it contains all hit terms
        if (currentSnippet.Length > 0 && foundHitTerms.SetEquals(_hitTerms))
        {
            snippets.Add(AdjustSnippetToLength(currentSnippet.ToString(), text, snippetStartOffset, currentOffset, fragmentLength));
        }

        tokenStream.End();
        tokenStream.Dispose();

        return snippets;
    }


    private string AdjustSnippetToLength(string snippet, string text, int snippetStartOffset, int snippetEndOffset, int fragmentLength)
    {
        int snippetLength = snippetEndOffset - snippetStartOffset;

        if (snippetLength >= fragmentLength)
        {
            return snippet;
        }

        // Calculate additional length needed
        int additionalLength = fragmentLength - snippetLength;
        int startIndex = Math.Max(snippetStartOffset - additionalLength / 2, 0);
        int endIndex = Math.Min(snippetEndOffset + additionalLength / 2, text.Length);

        return text.Substring(startIndex, endIndex - startIndex);
    }

    private IEnumerable<string> GetHitTermsForDoc(Query query, IndexSearcher searcher, int docId)
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
                hitTerms.Add(term.Text);
            }
        }
        return hitTerms;
    }

    void AppendTerm(StringBuilder builder, string term)
    {
        builder.Append(term + " ");
    }

    private string HighlightToken(string token)
    {
        return $"<mark>{token}</mark>";
    }
}
