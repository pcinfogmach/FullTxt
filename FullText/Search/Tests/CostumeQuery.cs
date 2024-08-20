using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

public class CustomQuery : Query
{
    private readonly string field;
    private readonly string value;

    public CustomQuery(string field, string value)
    {
        this.field = field;
        this.value = value;
    }

    public override Weight CreateWeight(IndexSearcher searcher)
    {
        return new CustomWeight(this, searcher, field, value);
    }

    public override string ToString(string field)
    {
        return $"CustomQuery(field: {this.field}, value: {this.value})";
    }
}

public class CustomWeight : Weight
{
    private readonly Query query;
    private readonly string field;
    private readonly string value;

    public CustomWeight(Query query, IndexSearcher searcher, string field, string value)
    {
        this.query = query;
        this.field = field;
        this.value = value;
    }

    public override Scorer GetScorer(AtomicReaderContext context, IBits acceptDocs)
    {
        var docsEnum = MultiFields.GetTermDocsEnum(context.Reader, null, field, new BytesRef(value));
        return docsEnum == null ? null : new CustomScorer(this, docsEnum);
    }

    public override Explanation Explain(AtomicReaderContext context, int doc)
    {
        // Since we are ignoring frequency and other factors, provide a simple explanation
        var explanation = new Explanation
        {
            Value = 1.0f,
            Description = "Match found, no additional factors considered."
        };
        return explanation;
    }

    public override float GetValueForNormalization() => 1f;

    public override void Normalize(float norm, float boost) { }

    public override Query Query => this.query;
}

public class CustomScorer : Scorer
{
    private readonly DocsEnum docsEnum;
    private int docId;

    public CustomScorer(Weight weight, DocsEnum docsEnum) : base(weight)
    {
        this.docsEnum = docsEnum;
        this.docId = -1;
    }

    public override int DocID => docId;

    public override int NextDoc()
    {
        docId = docsEnum.NextDoc();
        return docId;
    }

    public override int Advance(int target)
    {
        docId = docsEnum.Advance(target);
        return docId;
    }

    public override int Freq => 1; // Always return 1 since we're ignoring frequency

    public override float GetScore()
    {
        return 1.0f; // Return a constant score for each matching document
    }

    public override long GetCost()
    {
        // Return a constant value or some calculated cost
        return 1; // For simplicity, returning a constant value
    }
}
