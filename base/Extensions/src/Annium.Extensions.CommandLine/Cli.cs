using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Linq;

namespace Annium.Extensions.CommandLine;

/// <summary>
/// Provides utility methods for command line interface operations
/// </summary>
public static class Cli
{
    /// <summary>
    /// Clears the console screen
    /// </summary>
    public static void Clear()
    {
        Console.SetCursorPosition(0, 0);
        var clr = Enumerable
            .Range(0, Console.WindowHeight)
            .Select(_ => new string(' ', Console.WindowWidth))
            .Join(Environment.NewLine);
        Console.Write(clr);
        Console.SetCursorPosition(0, 0);
    }

    /// <summary>
    /// Prompts user for confirmation with Y/N input
    /// </summary>
    /// <param name="label">The confirmation prompt text</param>
    /// <param name="defaultValue">The default value if Enter is pressed</param>
    /// <returns>True if user confirms, false otherwise</returns>
    public static bool Confirm(string label, bool? defaultValue = null)
    {
        var y = defaultValue.HasValue && defaultValue.Value ? 'Y' : 'y';
        var n = defaultValue.HasValue && !defaultValue.Value ? 'N' : 'n';

        Console.WriteLine($"{label} ({y}/{n})");

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Y)
                return true;
            if (key.Key == ConsoleKey.N)
                return false;
            if (defaultValue.HasValue && key.Key == ConsoleKey.Enter)
                return defaultValue.Value;
        }
    }

    /// <summary>
    /// Prompts user for text input
    /// </summary>
    /// <param name="label">The prompt text</param>
    /// <returns>The user input text</returns>
    public static string Prompt(string label)
    {
        Console.Write(label);

        return Console.ReadLine() ?? string.Empty;
    }

    /// <summary>
    /// Reads secure input from user with masked characters
    /// </summary>
    /// <param name="label">The prompt text</param>
    /// <returns>The secure input text</returns>
    public static string ReadSecure(string label)
    {
        Console.Write(label);
        var result = new Stack<char>();
        var pos = Console.CursorLeft;
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Backspace:
                    if (result.Count > 0)
                    {
                        result.Pop();
                        Console.CursorLeft = --pos;
                        Console.Write(' ');
                        Console.CursorLeft = pos;
                    }

                    break;
                case ConsoleKey.Enter:
                    break;
                default:
                    result.Push(key.KeyChar);
                    Console.Write('*');
                    pos++;
                    break;
            }
        }
        // until enter is pressed
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();

        return string.Join(string.Empty, result.Reverse());
    }

    /// <summary>
    /// Writes colored text to the console
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    public static void WriteColored(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        using var _ = SetColors(foreground, background);
        Console.Write(text);
    }

    /// <summary>
    /// Writes colored text line to the console
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    public static void WriteLineColored(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        using var _ = SetColors(foreground, background);
        Console.WriteLine(text);
    }

    /// <summary>
    /// Sets console colors and returns a disposable to restore original colors
    /// </summary>
    /// <param name="foreground">The foreground color</param>
    /// <param name="background">The background color</param>
    /// <returns>A disposable that restores the original colors when disposed</returns>
    public static IDisposable SetColors(ConsoleColor? foreground = null, ConsoleColor? background = null)
    {
        var originalBackground = Console.BackgroundColor;
        if (background.HasValue)
            Console.BackgroundColor = background.Value;

        var originalForeground = Console.ForegroundColor;
        if (foreground.HasValue)
            Console.ForegroundColor = foreground.Value;

        return Disposable.Create(() =>
        {
            Console.BackgroundColor = originalBackground;
            Console.ForegroundColor = originalForeground;
        });
    }
}
