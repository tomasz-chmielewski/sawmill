using System;

namespace Sawmill.Common.Console
{
    /// <summary>
    /// Extension methods for the <see cref="System.Console"/> class.
    /// </summary>
    public static class ConsoleEx
    {
        /// <summary>
        /// Writes the specified string value to the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public static void Write(string value)
        {
            System.Console.Write(value);
        }

        /// <summary>
        /// Writes the current line terminator to the standard output stream.
        /// </summary>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public static void WriteLine()
        {
            System.Console.WriteLine();
        }

        /// <summary>
        /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public static void WriteLine(string value)
        {
            System.Console.WriteLine(value);
        }

        /// <summary>
        /// Writes the specified string value, with the specified foreground color, to the standard output stream.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="System.ArgumentException">The color specified in a set operation is not a valid member of System.ConsoleColor.</exception>
        /// <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public static void ColorWrite(ConsoleColor color, string value)
        {
            var previousColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.Write(value);
            System.Console.ForegroundColor = previousColor;
        }

        /// <summary>
        /// Writes the specified string value, with the specified foreground color, followed by the current line terminator, to the standard output stream.
        /// </summary>
        /// <param name="color">The foreground color.</param>
        /// <param name="value">The value to write.</param>
        /// <exception cref="System.ArgumentException">The color specified in a set operation is not a valid member of System.ConsoleColor.</exception>
        /// <exception cref="System.Security.SecurityException">The user does not have permission to perform this action.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred.</exception>
        public static void ColorWriteLine(ConsoleColor color, string value)
        {
            var previousColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(value);
            System.Console.ForegroundColor = previousColor;
        }
    }
}
