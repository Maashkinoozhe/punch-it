using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PunchItClient
{
    public class UserInterface
    {
        public static void Init()
        {
            Console.OutputEncoding=Encoding.UTF8;
        }

        public static string GetUserString(int indent, string question)
        {
            return GetUserString(indent, question, null, null);
        }

        public static string GetUserString(int indent, string question, Func<string, bool> validation, string allowedInputs)
        {
            string answer = null;
            while (answer == null)
            {
                PrintSameLine(indent, question);
                PrintSameLine(0, " -> ");
                var line = Console.In.ReadLine();
                if (validation != null)
                {
                    answer = (validation.Invoke(line)) ? line : null;
                }
                else
                {
                    answer = line;
                }

                if (answer == null)
                {
                    Print(indent, allowedInputs);
                    Print("");
                }
            }

            UserInterface.Print(indent, "");
            return answer;
        }

        public static string GetUserChar(int indent, string question, Func<string, bool> validation, Func<string, bool> stopRead, string allowedInputs, int allowedDigits)
        {
            string answer = null;
            while (answer == null)
            {
                PrintSameLine(indent, question);
                PrintSameLine(0, " -> ");
                var buffer = new char[allowedDigits];
                string line = null;
                var index = 0;
                while (index < allowedDigits)
                {
                    while (buffer[index] == '\0')
                    {
                        var key = Console.ReadKey();
                        buffer[index] = key.KeyChar;
                    }

                    line = new string(buffer.Take(index + 1).ToArray());
                    if (stopRead.Invoke(line)) break;
                    index += 1;
                }

                if (validation != null)
                {
                    answer = (validation.Invoke(line)) ? line : null;
                }
                else
                {
                    answer = line;
                }

                if (answer == null)
                {
                    Print(indent, allowedInputs);
                    Print("");
                }
            }

            UserInterface.Print(indent, "");
            return answer;
        }

        public static int GetUserInt(int indent, string question)
        {
            return GetUserInt(indent, question, null, null);
        }

        public static int GetUserInt(int indent, string question, Func<int, bool> validation, string allowedInputs, string questionIndicator = " -> ")
        {
            int? result = null;
            while (result == null)
            {
                PrintSameLine(indent, question);
                PrintSameLine(0, questionIndicator);
                var line = Console.In.ReadLine();
                var parsed = int.TryParse(line, out var intValue);
                if (parsed)
                {
                    if (validation != null)
                    {
                        result = (validation.Invoke(intValue)) ? intValue : (int?)null;
                    }
                    else
                    {
                        result = intValue;
                    }
                }

                if (!result.HasValue)
                {
                    Print(indent, allowedInputs);
                    Print("");
                }
            }

            UserInterface.Print(indent, "");
            return result.Value;
        }

        public static bool GetUserConfirmation(int indent, string question)
        {
            bool? answer = null;
            while (answer == null)
            {
                PrintSameLine(indent, question);
                PrintSameLine(0, " -> ");
                var line = Console.In.ReadLine();
                answer = GetValidConfirmation(line);

                if (answer == null) Print(indent, "Answer: y/Y/yes or n/N/no");
            }

            UserInterface.Print(indent, "");
            return answer.Value;
        }

        public static bool? GetValidConfirmation(string msg)
        {
            msg = msg.ToLower();
            if (msg.Equals("y") || msg.Equals("yes")) return true;
            if (msg.Equals("n") || msg.Equals("no")) return false;
            return null;
        }

        public static void RunUserInteractiveMode(IInteractiveModeViewAdapter adapter)
        {
            while (!adapter.IsDone())
            {
                var segments = adapter.GetFrameSegments();
                Console.Clear();
                foreach (var segment in segments)
                {
                    Console.ResetColor();
                    if (segment.ColorF.HasValue) Console.ForegroundColor = segment.ColorF.Value;
                    if (segment.ColorB.HasValue) Console.ForegroundColor = segment.ColorB.Value;
                    Console.Write(segment.Text);
                }

                InteractiveUserAction? action = null;
                while (!action.HasValue)
                {
                    var key = Console.ReadKey();
                    if (key.Key == ConsoleKey.UpArrow) action = InteractiveUserAction.Up;
                    if (key.Key == ConsoleKey.DownArrow) action = InteractiveUserAction.Down;
                    if (key.Key == ConsoleKey.LeftArrow) action = InteractiveUserAction.Left;
                    if (key.Key == ConsoleKey.RightArrow) action = InteractiveUserAction.Right;
                    if (key.Key == ConsoleKey.PageUp) action = InteractiveUserAction.PageUp;
                    if (key.Key == ConsoleKey.PageDown) action = InteractiveUserAction.PageDown;
                    if (key.Key == ConsoleKey.Q) action = InteractiveUserAction.Quit;
                }
                adapter.HandleUserAction(action.Value);
            }
        }

        public static void Print(string message)
        {
            Print(0, message);
        }

        public static void Print(int indent, string message, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
        {
            var padding = string.Concat(Enumerable.Repeat("\t", indent));

            SetColor(textColor, backgroundColor);
            Console.Out.WriteLine(padding + message);
            ResetColor();
        }

        public static void PrintSameLine(int indent, string message, ConsoleColor? textColor = null, ConsoleColor? backgroundColor = null)
        {
            var padding = string.Concat(Enumerable.Repeat("\t", indent));

            SetColor(textColor, backgroundColor);
            Console.Out.Write(padding + message);
            ResetColor();
        }

        private static void SetColor(ConsoleColor? textColor, ConsoleColor? backgroundColor)
        {
            if (textColor.HasValue)
            {
                Console.ForegroundColor = textColor.Value;
            }

            if (backgroundColor.HasValue)
            {
                Console.BackgroundColor = backgroundColor.Value;
            }
        }

        private static void ResetColor()
        {
            Console.ResetColor();
        }

        public static void ClearConsole()
        {
            Console.Clear();
        }

        public static void Pause()
        {
            PrintSameLine(0, "Press any Key to continue.");
            Console.ReadKey();
        }
    }
}