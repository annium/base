using System.Threading;
using System.Threading.Channels;

// ReSharper disable once CheckNamespace
namespace System;

public static class WriteToChannelExtensions
{
    public static void WriteToChannel<T>(
        this IObservable<T> source,
        ChannelWriter<T> writer,
        CancellationToken ct
    )
        => source
            .Subscribe(x =>
            {
                if (!writer.TryWrite(x))
                    throw new InvalidOperationException("Failed to write to channel");
            }, ct);
}