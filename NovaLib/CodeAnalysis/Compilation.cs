using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis
{
    public sealed class Compilation
    {
        public Compilation(SyntaxTree syntax)
        {
            Syntax = syntax;
        }

        public SyntaxTree Syntax { get; }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            BoundGlobalScope globalScope = Binder.BindGlobalScope(Syntax.Root);

            ImmutableArray<Diagnostic> diagnostics = Syntax.Diagnostics.Concat(globalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);
            
            Evaluator evaluator = new Evaluator(globalScope.Expression, variables);
            object value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}