using System;

namespace Sawmill.Common.Console
{
    public static class ConsoleEx
    {
        public static void NewLine()
        {
            if (System.Console.CursorLeft != 0)
            {
                System.Console.WriteLine();
            }
        }

        public static void Write(string value)
        {
            System.Console.Write(value);
        }

        public static void WriteNewLine(string value)
        {
            NewLine();
            System.Console.WriteLine(value);
        }

        public static void ColorWrite(ConsoleColor color, string value)
        {
            var previousColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.Write(value);
            System.Console.ForegroundColor = previousColor;
        }

        public static void ColorWriteNewLine(ConsoleColor color, string value)
        {
            NewLine();

            var previousColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(value);
            System.Console.ForegroundColor = previousColor;
        }
    }
}
