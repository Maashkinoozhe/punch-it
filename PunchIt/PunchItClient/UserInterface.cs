using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace PunchItClient
{
    public class UserInterface
    {
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

        public static int GetUserInt(int indent, string question)
        {
            return GetUserInt(indent, question, null, null);
        }

        public static int GetUserInt(int indent, string question, Func<int, bool> validation, string allowedInputs,string questionIndicator = " -> ")
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

        public static void Print(string message)
        {
            Print(0, message);
        }

        public static void Print(int indent, string message)
        {
            var padding = string.Concat(Enumerable.Repeat("\t", indent));
            Console.Out.WriteLine(padding + message);
        }

        public static void PrintSameLine(int indent, string message)
        {
            var padding = string.Concat(Enumerable.Repeat("\t", indent));
            Console.Out.Write(padding + message);
        }
    }
}