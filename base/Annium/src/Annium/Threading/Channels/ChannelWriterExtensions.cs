using System;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Annium.Threading.Channels;

/// <summary>
/// Provides extension methods for working with channel writers.
/// </summary>
public static class ChannelWriterExtensions
{
    /// <summary>
    /// Writes an item to the channel writer.
    /// </summary>
    /// <typeparam name="T">The type of items in the channel.</typeparam>
    /// <param name="writer">The channel writer.</param>
    /// <param name="item">The item to write.</param>
    /// <exception cref="InvalidOperationException">Thrown when the write operation fails.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(this ChannelWriter<T> writer, T item)
    {
        if (!writer.TryWrite(item))
            throw new InvalidOperationException($"Failed to write to channel {item.GetFullId()}");
    }
}
