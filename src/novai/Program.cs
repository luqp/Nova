using Nova.CodeAnalysis.Binding;

namespace Nova
{
    internal static class Program
    {
        private static void Main()
        {
            NovaRepl repl = new NovaRepl();
            repl.Run();
        }
    }
}
