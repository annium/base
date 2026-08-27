using System;
using System.IO;
using Annium.Testing;
using Xunit;

namespace Annium.Extensions.CommandLine.Tests;

/// <summary>
/// Tests for the console-writing half of <see cref="Cli"/>. The reading half (Confirm, Prompt, ReadSecure)
/// blocks on real key presses and cannot run without an interactive console, so it stays uncovered.
/// </summary>
[Collection("console")]
public class CliTests
{
    /// <summary>
    /// Stepping the cursor back over a mask that wrapped goes up a row, rather than asking the console for
    /// a column it does not have. A flat column counter is what a masked prompt naively keeps, and it walks
    /// straight past the buffer width as soon as the secret is longer than the terminal is wide.
    /// </summary>
    [Fact]
    public void StepBack_FromWrappedColumn_GoesUpARow()
    {
        // arrange & act & assert - within a row, it is just the previous column
        Cli.StepBack(5, 3, 80).Is((4, 3));

        // at the start of a row, it is the last column of the row above
        Cli.StepBack(0, 3, 80).Is((79, 2));

        // at the very start of the buffer there is nowhere to go, and asking for one would throw
        Cli.StepBack(0, 0, 80).Is((0, 0));
    }

    /// <summary>
    /// Colors set for the duration of a write are put back afterwards, so a coloured line does not tint
    /// everything printed after it.
    /// </summary>
    [Fact]
    public void SetColors_Disposed_RestoresPreviousColors()
    {
        // arrange
        var foreground = Console.ForegroundColor;
        var background = Console.BackgroundColor;

        // act
        using (Cli.SetColors(ConsoleColor.Red, ConsoleColor.Blue))
        {
            Console.ForegroundColor.Is(ConsoleColor.Red);
            Console.BackgroundColor.Is(ConsoleColor.Blue);
        }

        // assert
        Console.ForegroundColor.Is(foreground);
        Console.BackgroundColor.Is(background);
    }

    /// <summary>
    /// Only the colors actually asked for are changed.
    /// </summary>
    [Fact]
    public void SetColors_OnlyForeground_LeavesBackgroundAlone()
    {
        // arrange
        var background = Console.BackgroundColor;

        // act & assert
        using (Cli.SetColors(ConsoleColor.Green))
        {
            Console.ForegroundColor.Is(ConsoleColor.Green);
            Console.BackgroundColor.Is(background);
        }
    }

    /// <summary>
    /// Coloured writing emits the text itself, with and without a trailing newline.
    /// </summary>
    [Fact]
    public void WriteColored_WritesTheText()
    {
        // arrange
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // act
            Cli.WriteColored("plain", ConsoleColor.Yellow);
            Cli.WriteLineColored("line", ConsoleColor.Cyan);

            // assert
            var written = writer.ToString();
            written.Contains("plain").IsTrue("written text must reach the console");
            written.Contains("line").IsTrue("written line must reach the console");
            written.EndsWith(Environment.NewLine).IsTrue("WriteLineColored must end the line");
        }
        finally
        {
            Console.SetOut(original);
        }
    }
}
