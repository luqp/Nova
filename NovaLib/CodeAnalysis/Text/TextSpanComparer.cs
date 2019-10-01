using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Nova.CodeAnalysis.Text
{
    public class TextSpanComparer : IComparer<TextSpan>
    {
        public int Compare(TextSpan x, TextSpan y)
        {
            int textCompared = x.Start - y.Start;
            if (textCompared == 0)
                textCompared = x.Length - y.Length;

            return textCompared;
        }
    }
}