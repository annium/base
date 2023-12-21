using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Annium.Threading.Channels;

public static class ChannelReaderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Read<T>(this ChannelReader<T> reader)
    {
        if (!reader.TryRead(out var item))
            throw new InvalidOperationException($"Failed to write to channel {item.GetFullId()}");

        return item;
    }
}
