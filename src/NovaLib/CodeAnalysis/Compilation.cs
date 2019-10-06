using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Nova.CodeAnalysis.Binding;
using Nova.CodeAnalysis.Symbols;
using Nova.CodeAnalysis.Syntax;

namespace Nova.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope globalScope;

        public Compilation(SyntaxTree syntaxTree)
        : this(null, syntaxTree)
        {
        }

        private Compilation(Compilation previous, SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        public Compilation Previous { get; }
        public SyntaxTree SyntaxTree { get; }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (globalScope == null)
                {
                    BoundGlobalScope scope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                    Interlocked.CompareExchange(ref globalScope, scope, null);
                }
                return globalScope;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            ImmutableArray<Diagnostic> diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);

            BoundProgram program = Binder.BindProgram(GlobalScope);

            string appPath = Environment.GetCommandLineArgs()[0];
            string appDirectory = Path.GetDirectoryName(appPath);
            string cfgPath = Path.Combine(appDirectory, "cfg.dot");
            BoundBlockStatement cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
                                                  ? program.Functions.Last().Value
                                                  : program.Statement;
            var cfg = ControlFlowGraph.Create(cfgStatement);
            using (StreamWriter streamWriter = new StreamWriter(cfgPath))
                cfg.WriteTo(streamWriter);

            if (program.Diagnostics.Any())
                return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

            Evaluator evaluator = new Evaluator(program, variables);
            object value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            BoundProgram program = Binder.BindProgram(GlobalScope);
           
            if (program.Statement.Statements.Any())
            {
                program.Statement.WriteTo(writer);
            }
            else
            {
                foreach (var functionBody in program.Functions)
                {
                    if (!GlobalScope.Functions.Contains(functionBody.Key))
                        continue;
                    
                    functionBody.Key.WriteTo(writer);
                    functionBody.Value.WriteTo(writer);
                }
            }
        }
    }
}