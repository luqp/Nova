namespace Nova.CodeAnalysis.Text
{
    public sealed class TextLine
    {
        public TextLine(SourceText text, int start, int length, int lengthInclundingLineBreak)
        {
            Text = text;
            Start = start;
            Length = length;
            LengthInclundingLineBreak = lengthInclundingLineBreak;
        }

        public SourceText Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public int LengthInclundingLineBreak { get; }
        public TextSpan Span => new TextSpan(Start, Length);
        public TextSpan SpanIncludingLineBreal => new TextSpan(Start, LengthInclundingLineBreak);
    }
}