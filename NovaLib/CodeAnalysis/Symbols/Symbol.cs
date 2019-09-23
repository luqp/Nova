namespace Nova.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        public Symbol(string name)
        {
            Name = name;
        }

        public abstract SymbolKind Kind { get; }
        public string Name { get; }
        public override string ToString() => Name;
    }
}