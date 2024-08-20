using System.Collections.Generic;

internal interface ICustomHighlighter
{
    List<string> HighlightText(string fieldName, int fragmentLength);
}