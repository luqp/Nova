using System;
using System.Text;

namespace Nova
{
    internal abstract class Repl
    {
        private readonly StringBuilder textBuilder = new StringBuilder();

        public void Run()
        {
            Console.WriteLine("Commands: #trees, #cls");

            while (true)
            {
                if (textBuilder.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("» ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("· ");
                }
                Console.ResetColor();

                string input = Console.ReadLine();
                bool isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    else if (input.StartsWith("#"))
                    {
                        EvaluateMetaCommand(input);
                        Console.ResetColor();
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                string text = textBuilder.ToString();

                if (!IsCompleteSubmission(text))
                    continue;

                EvaluateSubmission(text);

                textBuilder.Clear();
            }
        }

        protected virtual void EvaluateMetaCommand(string input)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Invalid command {input}.");
            Console.ResetColor();
        }

        protected abstract bool IsCompleteSubmission(string text);

        protected abstract void EvaluateSubmission(string text);

    }
}
