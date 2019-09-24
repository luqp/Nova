using System;

namespace Nova.CodeAnalysis.Symbols
{
    public sealed class ParameterSymbol : VariableSymbol
    {
        internal ParameterSymbol(string name, TypeSymbol type)
            : base(name, isReadOnly: true, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}