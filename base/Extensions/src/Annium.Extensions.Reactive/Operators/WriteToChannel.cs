using System.Threading;
using System.Threading.Channels;
using Annium.Threading.Channels;

// ReSharper disable once CheckNamespace
namespace System;

public static class WriteToChannelExtensions
{
    public static void WriteToChannel<T>(this IObservable<T> source, ChannelWriter<T> writer, CancellationToken ct) =>
        source.Subscribe(writer.Write, ct);
}
