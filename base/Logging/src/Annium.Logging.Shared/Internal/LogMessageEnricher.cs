using System;
using System.Text;

namespace Annium.Logging.Shared.Internal;

/// <summary>
/// Helper class for enriching log messages with exception information
/// </summary>
internal static class LogMessageEnricher
{
    /// <summary>
    /// Gets a formatted exception message, handling aggregate exceptions specially
    /// </summary>
    /// <param name="exception">The exception to format</param>
    /// <returns>A formatted exception message</returns>
    public static string GetExceptionMessage(Exception exception)
    {
        if (exception is not AggregateException aggregateException)
            return GetBaseExceptionMessage(exception);

        var errors = aggregateException.Flatten().InnerExceptions;

        var sb = new StringBuilder($"{errors.Count} error(s) in: {GetBaseExceptionMessage(aggregateException)}");
        foreach (var error in errors)
            sb.AppendLine(GetBaseExceptionMessage(error));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the base exception message including message and stack trace
    /// </summary>
    /// <param name="e">The exception to format</param>
    /// <returns>A formatted exception message with stack trace</returns>
    private static string GetBaseExceptionMessage(Exception e) => $"{e.Message}{e.StackTrace}";
}
