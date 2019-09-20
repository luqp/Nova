using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Nova
{
    internal abstract class Repl
    {
        private bool done;

        public void Run()
        {
            while (true)
            {
                string text = EditSubmission();
                if (string.IsNullOrEmpty(text))
                    return;
                
                EvaluateSubmission(text);
            }
        }

        private sealed class SubmissionView
        {
            private readonly ObservableCollection<string> submissionDocument;
            private readonly int cursorTop;
            private int renderedLineCount;
            private int currentLineIndex;
            private int currentCharacter;

            public SubmissionView(ObservableCollection<string> submissionDocument)
            {
                this.submissionDocument = submissionDocument;
                this.submissionDocument.CollectionChanged += SubmissionDocumentChanged;
                cursorTop = Console.CursorTop;
                Render();
            }

            private void SubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                Render();
            }

            private void Render()
            {
                Console.SetCursorPosition(0, cursorTop);
                Console.CursorVisible = false;

                int lineCount = 0;

                foreach (string line in submissionDocument)
                {
                    if (lineCount == 0)
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
                    Console.WriteLine(line);
                    lineCount++;
                }

                int numberOfBlankLines = renderedLineCount - lineCount;

                if (numberOfBlankLines > 0)
                {
                    string blankLine = new string(' ', Console.WindowWidth);
                    while (numberOfBlankLines > 0)
                    {
                        Console.WriteLine(blankLine);
                    }
                }

                renderedLineCount = lineCount;
                Console.CursorVisible = true;

                UpdateCursorPosition();

            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = cursorTop + currentLineIndex;
                Console.CursorLeft = 2 + currentCharacter;
            }

            public int CurrentLineIndex
            {
                get => currentLineIndex;
                set
                {
                    if (currentLineIndex != value)
                    {
                        currentLineIndex = value;
                        UpdateCursorPosition();
                    }
                }
            }
            public int CurrentCharacter
            {
                get => currentCharacter;
                set
                {
                    if (currentCharacter != value)
                    {
                        currentCharacter = value;
                        UpdateCursorPosition();
                    }
                }
            }
        }

        private string EditSubmission()
        {
            done = false;
            ObservableCollection<string> document = new ObservableCollection<string>() { "" };
            SubmissionView view = new SubmissionView(document);

            while (!done)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                HandleKey(key, document, view);
            }

            Console.WriteLine();

            return string.Join(Environment.NewLine, document);
        }

        private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionView view)
        {
            if (key.Modifiers == default(ConsoleModifiers))
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleEnter(document, view);
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(document, view);
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, view);
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow(document, view);
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, view);
                        break;
                }
            }
            else if (key.Modifiers == ConsoleModifiers.Control)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleControlEnter(document, view);
                        break;
                }
            }
            if (key.KeyChar >= ' ')
                HandleTyping(document, view, key.KeyChar.ToString());
        }

        private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
        {
            string submissionText = string.Join(Environment.NewLine, document);
            if (IsCompleteSubmission(submissionText))
            {
                done = true;
                return;
            }

            document.Add(string.Empty);
            view.CurrentCharacter = 0;
            view.CurrentLineIndex = document.Count - 1;
        }

        private void HandleControlEnter(ObservableCollection<string> document, SubmissionView view)
        {
            done = true;
        }

        private void HandleLeftArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentCharacter > 0)
                view.CurrentCharacter--;
        }

        private void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
        {
            string line = document[view.CurrentLineIndex];
            if (view.CurrentCharacter < line.Length - 1)
                view.CurrentCharacter++;
        }

        private void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex > 0)
                view.CurrentLineIndex--;
        }

        private void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex < document.Count - 1)
                view.CurrentLineIndex++;
        }

        private void HandleTyping(ObservableCollection<string> document, SubmissionView view, string text)
        {
            int lineIndex = view.CurrentLineIndex;
            int start = view.CurrentCharacter;

            document[lineIndex] = document[lineIndex].Insert(start, text);
            view.CurrentCharacter += text.Length; 
        }

        private string EditSubmissionOld()
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
