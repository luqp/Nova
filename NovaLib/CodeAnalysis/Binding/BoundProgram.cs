using System.Collections.Immutable;
using Nova.CodeAnalysis.Symbols;

namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram(BoundGlobalScope globalScope, DiagnosticBag diagnostics, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies)
        {
            GlobalScope = globalScope;
            Diagnostics = diagnostics;
            FunctionBodies = functionBodies;
        }

        public BoundGlobalScope GlobalScope { get; }
        public DiagnosticBag Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies { get; }
    }
}