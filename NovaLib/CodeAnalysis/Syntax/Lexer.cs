using System;
using System.Collections.Generic;
using System.Text;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Text;

namespace Nova.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly SourceText text;
        private readonly DiagnosticBag diagnostics = new DiagnosticBag();
        private int position;

        private int start;
        private SyntaxKind kind;
        private object value;

        public Lexer(SourceText text)
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
                case '{':
                    kind = SyntaxKind.OpenBraceToken;
                    position++;
                    break;
                case '}':
                    kind = SyntaxKind.CloseBraceToken;
                    position++;
                    break;
                case '~':
                    kind = SyntaxKind.TildeToken;
                    position++;
                    break;
                case '^':
                    kind = SyntaxKind.HatToken;
                    position++;
                    break;
                case '&':
                    position++;
                    if (Current != '&')
                    {
                        kind = SyntaxKind.AmpersandToken;
                    }
                    else
                    {
                        kind = SyntaxKind.AmpersandAmpersandToken;
                        position++;
                    }
                    break;
                case '|':
                    position++;
                    if (Current != '|')
                    {
                        kind = SyntaxKind.PipeToken;
                    }
                    else
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
                case '<':
                    position++;
                    if (Current != '=')
                    {
                        kind = SyntaxKind.LessToken;
                    }
                    else
                    {
                        kind = SyntaxKind.LessOrEqualsToken;
                        position++;
                    }
                    break;
                case '>':
                    position++;
                    if (Current != '=')
                    {
                        kind = SyntaxKind.GreaterToken;
                    }
                    else
                    {
                        kind = SyntaxKind.GreaterOrEqualsToken;
                        position++;
                    }
                    break;
                case '"':
                    ReadString();
                    break;
                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    ReadNumberToken();
                    break;
                case ' ':
                case '\t':
                case '\n':
                case '\r':
                    ReadWhiteSpace();
                    break;
                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
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
                tokenText = text.ToString(start, length);

            return new SyntaxToken(kind, start, tokenText, value);
        }

        private void ReadString()
        {
            position++;
            StringBuilder sb = new StringBuilder();
            bool done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        TextSpan span = new TextSpan(start, 1);
                        diagnostics.ReportUnterminatedString(span);
                        done = true;
                        break;
                    case '"':
                        position++;
                        if (Current == '"')
                        {
                            sb.Append(Current);
                            position++;
                        }
                        else
                        {
                            done = true;
                        }
                        break;
                    default:
                        sb.Append(Current);
                        position++;
                        break;
                }
            }

            kind = SyntaxKind.StringToken;
            value = sb.ToString();
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
            string tokenText = text.ToString(start, length);
            if (!int.TryParse(tokenText, out var tokenValue))
                diagnostics.ReportInvalidNumber(new TextSpan(start, length), tokenText, TypeSymbol.Int);

            value = tokenValue;
            kind = SyntaxKind.NumberToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                Next();
            int length = position - start;
            string tokenText = text.ToString(start, length);
            kind = SyntaxFacts.GetKeywordKind(tokenText);
        }
    }
}