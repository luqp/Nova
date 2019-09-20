using System;
using System.Text;

namespace Nova
{
    internal abstract class Repl
    {
        public void Run()
        {
            while (true)
            {
                string text = EditSubmission();
                if (text == null)
                    return;

                EvaluateSubmission(text);
            }
        }

        private string EditSubmission()
        {
            StringBuilder textBuilder = new StringBuilder();

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
                        return null;
                    }
                    if (input.StartsWith("#"))
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

                return text;
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
