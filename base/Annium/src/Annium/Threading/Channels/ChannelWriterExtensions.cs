using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Annium.Threading.Channels;

public static class ChannelWriterExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(this ChannelWriter<T> writer, T item)
    {
        if (!writer.TryWrite(item))
            throw new InvalidOperationException($"Failed to write to channel {item.GetFullId()}");
    }
}
