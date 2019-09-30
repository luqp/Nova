using System.Collections.Immutable;
using Nova.CodeAnalysis.Symbols;

namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, BoundBlockStatement statement)
        {
            Diagnostics = diagnostics;
            Functions = functions;
            Statement = statement;
        }

        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public BoundBlockStatement Statement { get; }
    }
}