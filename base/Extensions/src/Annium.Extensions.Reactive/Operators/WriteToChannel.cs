using System;
using System.Threading;
using System.Threading.Channels;
using Annium.Threading.Channels;

namespace Annium.Extensions.Reactive.Operators;

/// <summary>
/// Provides extension methods for writing observable values to channels
/// </summary>
public static class WriteToChannelExtensions
{
    /// <summary>
    /// Writes all values emitted by an observable to a channel writer
    /// </summary>
    /// <typeparam name="T">The type of items emitted by the observable</typeparam>
    /// <param name="source">The source observable to read from</param>
    /// <param name="writer">The channel writer to write values to</param>
    /// <param name="ct">Cancellation token for the subscription</param>
    public static void WriteToChannel<T>(this IObservable<T> source, ChannelWriter<T> writer, CancellationToken ct) =>
        source.Subscribe(writer.Write, ct);
}
