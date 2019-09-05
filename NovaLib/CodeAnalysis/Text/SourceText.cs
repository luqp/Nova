using System;
using System.Collections.Immutable;

namespace Nova.CodeAnalysis.Text
{
    public sealed class SourceText
    {
        private readonly string text;

        private SourceText(string text)
        {
            this.text = text;
            Lines = ParseLines(this, text);
        }

        public ImmutableArray<TextLine> Lines { get; }

        public int GetLineIndex(int position)
        {
            int lower = 0;
            int upper = text.Length - 1;

            while (lower <= upper)
            {
                int index = lower + (upper - lower) / 2;
                int start = Lines[index].Start;

                if (position == start)
                    return index;
                if (position > start)
                    lower = index + 1;
                else
                    upper = index - 1;
            }
            return lower - 1; 
        }

        private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();
            int position = 0;
            int lineStart = 0;

            while (position < text.Length)
            {
                int lineBreakWidth = GetLineBreakWidth(text, position);

                if (lineBreakWidth == 0)
                {
                    position++;
                }
                else
                {
                    AddLine(result, sourceText, position, lineStart, lineBreakWidth);
                    
                    position += lineBreakWidth;
                    lineStart = position;
                }
            }

            if (position > lineStart)
                AddLine(result, sourceText, position, lineStart, 0);

            return result.ToImmutable();
        }

        private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
        {
            int lineLength = position - lineStart;
            int lineLengthIncludingLineBreack = lineLength + lineBreakWidth;
            TextLine line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreack);
            result.Add(line);
        }

        private static int GetLineBreakWidth(string text, int position)
        {
            char character = text[position];
            char lookAhead = position + 1 >= text.Length ? '\0' : text[position + 1];

            if (character == '\r' && lookAhead == '\n')
                return 2;
            if (character == '\r' || lookAhead == '\n')
                return 1;
            
            return 0;
        }

        public static SourceText From(string text)
        {
            return new SourceText(text);
        }
    }
}