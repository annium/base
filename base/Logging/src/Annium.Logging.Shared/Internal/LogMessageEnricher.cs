using System;
using System.Text;

namespace Annium.Logging.Shared.Internal;

internal static class LogMessageEnricher
{
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

    private static string GetBaseExceptionMessage(Exception e) => $"{e.Message}{e.StackTrace}";
}