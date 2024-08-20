using Lucene.Net.Index;
using Lucene.Net.Search.Similarities;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FullText.Search
{
    public class CustomSimilarity : DefaultSimilarity
    {
        public override float Tf(float freq)
        {
            return 1.0f; // Ignores term frequency
        }

        public override float Idf(long docFreq, long docCount)
        {
            return 1.0f; // Ignores inverse document frequency
        }

        public override float LengthNorm(FieldInvertState state)
        {
            return 1.0f; // Ignores the length of the field
        }
    }
}
