using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Nova.CodeAnalysis.Symbols;

namespace Nova.CodeAnalysis.Binding
{
    internal sealed class BoundScope
    {
        private Dictionary<string, Symbol> symbols;

        public BoundScope(BoundScope parent)
        {
            Parent = parent;
        }

        public BoundScope Parent { get; }

        private bool TryDeclareSymbol<TSymbol>(TSymbol symbol)
            where TSymbol : Symbol
        {
            if (symbols == null)
                symbols = new Dictionary<string, Symbol>();
            else if (symbols.ContainsKey(symbol.Name))
                return false;
            
            symbols.Add(symbol.Name, symbol);
            return true;
        }

        private bool TryLookupSymbol<TSymbol>(string name, out TSymbol symbol)
            where TSymbol: Symbol
        {
            symbol = null;

            if (symbols != null && symbols.TryGetValue(name, out var symbolValue))
            {
                if (symbolValue is TSymbol result)
                {
                    symbol = result;
                    return true;
                }

                return false;
            }

            if (Parent == null)
                return false;
            
            return Parent.TryLookupSymbol(name, out symbol);
        }

        public bool TryDeclareVariable(VariableSymbol variable)
            => TryDeclareSymbol(variable);

        public bool TryLookupVariable(string name, out VariableSymbol variable)
            => TryLookupSymbol(name, out variable);

        public bool TryDeclareFunction(FunctionSymbol function)
            => TryDeclareSymbol(function);

        public bool TryLookupFunction(string name, out FunctionSymbol function)
            => TryLookupSymbol(name, out function);

        public ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>()
            where TSymbol : Symbol
        {
            if (symbols == null)
                return ImmutableArray<TSymbol>.Empty;

            return symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
            => GetDeclaredSymbols<VariableSymbol>();

        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
            => GetDeclaredSymbols<FunctionSymbol>();
    }
}