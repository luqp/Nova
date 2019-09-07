using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope globalScope;

        public Compilation(SyntaxTree syntax)
        {
            Syntax = syntax;
        }

        public SyntaxTree Syntax { get; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (globalScope == null)
                {
                    BoundGlobalScope scope = Binder.BindGlobalScope(Syntax.Root);
                    Interlocked.CompareExchange(ref globalScope, scope, null);
                }
                return globalScope;
            }
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            ImmutableArray<Diagnostic> diagnostics = Syntax.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);
            
            Evaluator evaluator = new Evaluator(GlobalScope.Expression, variables);
            object value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}