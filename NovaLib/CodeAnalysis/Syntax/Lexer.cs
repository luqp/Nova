using System.Collections.Generic;

namespace Nova.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly string text;
        private readonly DiagnosticBag diagnostics = new DiagnosticBag();
        private int position;

        private int start;
        private SyntaxKind kind;
        private object value;

        public Lexer(string text)
        {
            this.text = text;
        }

        public DiagnosticBag Diagnostics => diagnostics;

        private char Current => Peek(0);

        private char Peek(int offset)
        {
            int index = position + offset;
            if (index >= text.Length)
                return '\0';
            return text[index];
        }

        private void Next()
        {
            position++;
        }

        public SyntaxToken Lex()
        {
            start = position;
            kind = SyntaxKind.BadToken;
            value = null;

            switch (Current)
            {
                case '\0':
                    kind = SyntaxKind.EndOfFileToken;
                    position++;
                    break;
                case '+':
                    kind = SyntaxKind.PlusToken;
                    position++;
                    break;
                case '-':
                    kind = SyntaxKind.MinusToken;
                    position++;
                    break;
                case '*':
                    kind = SyntaxKind.StarToken;
                    position++;
                    break;
                case '/':
                    kind = SyntaxKind.SlashToken;
                    position++;
                    break;
                case '(':
                    kind = SyntaxKind.OpenParenthesisToken;
                    position++;
                    break;
                case ')':
                    kind = SyntaxKind.CloseParenthesisToken;
                    position++;
                    break;
                case '&':
                    position++;
                    if (Current == '&')
                    {
                        kind = SyntaxKind.AmpersanAmpersanToken;
                        position++;
                    }
                    break;
                case '|':
                    position++;
                    if (Current == '|')
                    {
                        kind = SyntaxKind.PipePipeToken;
                        position++;
                    }
                    break;
                case '=':
                    position++;
                    if (Current != '=')
                    {
                        kind = SyntaxKind.EqualsToken;
                    }
                    else
                    {
                        kind = SyntaxKind.EqualsEqualsToken;
                        position++;
                    }
                    break;
                case '!':
                    position++;
                    if (Current != '=')
                    {
                        kind = SyntaxKind.BangToken;
                    }
                    else
                    {
                        kind = SyntaxKind.BangEqualsToken;
                        position++;
                    }
                    break;
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    ReadNumberToken();
                    break;
                default:
                    if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
                    }
                    else if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else
                    {
                        diagnostics.ReportBadCharacter(position, Current);
                        position++;
                    }
                    break;
            }

            int length = position - start;
            string tokenText = SyntaxFacts.GetText(kind);

            if (tokenText == null)
                tokenText = text.Substring(start, length);

            return new SyntaxToken(kind, start, tokenText, value);
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                Next();
            kind = SyntaxKind.WhiteSpaceToken;
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
                Next();
            int length = position - start;
            string tokenText = text.Substring(start, length);
            if (!int.TryParse(tokenText, out var tokenValue))
                diagnostics.ReportInvalidNumber(new TextSpan(start, length), tokenText, typeof(int));

            value = tokenValue;
            kind = SyntaxKind.NumberToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                Next();
            int length = position - start;
            string tokenText = text.Substring(start, length);
            kind = SyntaxFacts.GetKeywordKind(tokenText);
        }
    }
}