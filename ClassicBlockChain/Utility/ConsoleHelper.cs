using System;

namespace UChainDB.Example.Chain.Utility
{
    public static class ConsoleHelper
    {
        public static void WriteLine(string value, int number = 0)
        {
            Write(value, number);
            Console.WriteLine();
        }

        static ConsoleColor[] colors = new[] {
            ConsoleColor.White,
            ConsoleColor.Yellow,
            ConsoleColor.Magenta,
            ConsoleColor.Green,
            ConsoleColor.Cyan,
            ConsoleColor.Red,
            ConsoleColor.Gray,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
        };

        public static void Write(string value, int number = 0)
        {
            var color = colors[number % colors.Length];
            Write(value, color);
        }

        public static void WriteLine(string value, ConsoleColor color, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Write(value, color, backgroundColor);
            Console.WriteLine();
        }

        public static void Write(string value, ConsoleColor color, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = backgroundColor;
            Console.Write(value);
            Console.ResetColor();
        }
    }
}