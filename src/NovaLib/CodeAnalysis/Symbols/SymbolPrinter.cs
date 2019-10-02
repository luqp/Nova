using System;
using System.IO;
using Nova.CodeAnalysis.Syntax;
using Nova.IO;

namespace Nova.CodeAnalysis.Symbols
{
    internal static class SymbolPrinter
    {
        public static void WriteTo(Symbol symbol, TextWriter writer)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Function:
                    WriteFunction((FunctionSymbol)symbol, writer);
                    break;
                case SymbolKind.GlobalVariable:
                    WriteGlobalVariable((GlobalVariableSymbol)symbol, writer);
                    break;
                case SymbolKind.LocalVariable:
                    WriteLocalVariable((LocalVariableSymbol)symbol, writer);
                    break;
                case SymbolKind.Parameter:
                    WriteParameter((ParameterSymbol)symbol, writer);
                    break;
                case SymbolKind.Type:
                    WriteType((TypeSymbol)symbol, writer);
                    break;
                default:
                    throw new Exception($"Unexpected symbol: {symbol.Kind}");
            }
        }

        private static void WriteFunction(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword(SyntaxKind.FunctionKeyword);
            writer.WriteWhiteSpace();
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(SyntaxKind.OpenParenthesisToken);

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                if (i > 0)
                    {
                        writer.WritePunctuation(SyntaxKind.CommaToken);
                        writer.WriteWhiteSpace();
                    }
                
                symbol.Parameters[i].WriteTo(writer);
            }

            writer.WritePunctuation(SyntaxKind.CloseParenthesisToken);
            writer.WriteLine();
        }

        private static void WriteGlobalVariable(GlobalVariableSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword(symbol.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword);
            writer.WriteWhiteSpace();
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(SyntaxKind.ColonToken);
            writer.WriteWhiteSpace();
            symbol.Type.WriteTo(writer);
        }

        private static void WriteLocalVariable(LocalVariableSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword(symbol.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword);
            writer.WriteWhiteSpace();
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(SyntaxKind.ColonToken);
            writer.WriteWhiteSpace();
            symbol.Type.WriteTo(writer);
        }

        private static void WriteParameter(ParameterSymbol symbol, TextWriter writer)
        {
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(SyntaxKind.ColonToken);
            writer.WriteWhiteSpace();
            symbol.Type.WriteTo(writer);
        }

        private static void WriteType(TypeSymbol symbol, TextWriter writer)
        {
            writer.WriteIdentifier(symbol.Name);
        }
    }
}