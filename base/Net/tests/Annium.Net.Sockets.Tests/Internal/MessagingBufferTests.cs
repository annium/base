using System;
using System.Linq;
using Annium.Net.Sockets.Internal;
using Annium.Testing;
using Xunit;
using static Annium.Net.Sockets.Internal.MessagingBuffer;

namespace Annium.Net.Sockets.Tests.Internal;

public class MessagingBufferTests
{
    [Fact]
    public void No_Data()
    {
        using var buffer = new MessagingBuffer(2);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // reset
        buffer.Reset();
    }

    [Fact]
    public void Partial_Header()
    {
        using var buffer = new MessagingBuffer(2);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // write header partially
        buffer.Write(new byte[] { 1, 2 });

        // assert
        buffer.Assert(false, false, freeSpace - 2, Array.Empty<byte>());

        // reset
        buffer.Reset();
    }

    [Fact]
    public void Full_Header()
    {
        using var buffer = new MessagingBuffer(10);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // write header full
        buffer.WriteMessageSize(2);

        // assert
        buffer.Assert(false, false, freeSpace - HeaderSize, Array.Empty<byte>());

        // reset
        buffer.Reset();
    }

    [Fact]
    public void Partial_Data()
    {
        using var buffer = new MessagingBuffer(10);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // write header and partially data
        buffer.WriteMessageSize(2);
        buffer.Write(new byte[] { 1 });

        // assert
        buffer.Assert(false, false, freeSpace - HeaderSize - 1, Array.Empty<byte>());

        // reset
        buffer.Reset();
    }

    [Fact]
    public void Exact_Data()
    {
        using var buffer = new MessagingBuffer(10);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // write single message
        var message = new byte[] { 1, 3 };
        buffer.WriteMessage(message);

        // assert
        buffer.Assert(false, true, freeSpace - HeaderSize - message.Length, message);

        // reset
        buffer.Reset();

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());
    }

    [Fact]
    public void Extra_Data()
    {
        using var buffer = new MessagingBuffer(10);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // write full messageA
        var messageA = new byte[] { 1, 3 };
        buffer.WriteMessage(messageA);

        // write size of messageB and part of it's data
        var messageB = new byte[] { 7, 1, 4 };
        buffer.WriteMessageSize(messageB.Length);
        buffer.Write(messageB.AsSpan(0, 1));

        // assert - free space in end of buffer
        buffer.Assert(false, true, freeSpace - HeaderSize - messageA.Length - HeaderSize - 1, messageA);

        // reset - drops message, moves data to start, cause no more messages in buffer
        buffer.Reset();

        // assert - free space still in end of buffer (despite pointer moved in the buffer start)
        buffer.Assert(false, false, freeSpace - HeaderSize - 1, Array.Empty<byte>());

        // write messageB left body
        buffer.Write(messageB.AsSpan(1));

        // assert
        buffer.Assert(false, true, freeSpace - HeaderSize - messageB.Length, messageB);

        // reset
        buffer.Reset();

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());
    }

    [Fact]
    public void MultiMessage_Data()
    {
        using var buffer = new MessagingBuffer(20);
        var freeSpace = buffer.FreeSpace.Length;

        // assert
        buffer.Assert(false, false, freeSpace, Array.Empty<byte>());

        // write full messageA
        var messageA = new byte[] { 1, 3 };
        buffer.WriteMessage(messageA);

        // write full messageB
        var messageB = new byte[] { 7, 1, 4 };
        buffer.WriteMessage(messageB);

        // write size of messageC (exhausting buffer size) and part of it's data
        var messageC = Enumerable.Range(0, 200).Select(x => (byte)x).ToArray();
        buffer.WriteMessageSize(messageC.Length);
        var messageCBytes = buffer.FreeSpace.Length;
        buffer.Write(messageC.AsSpan(0, messageCBytes));

        // assert - full buffer with messageA
        buffer.Assert(true, true, 0, messageA);

        // reset - drops messageA
        buffer.Reset();

        // assert - full buffer with messageB
        buffer.Assert(true, true, 0, messageB);

        // reset - drops messageB, moves data to internal buffer start
        buffer.Reset();

        // assert - free space available, no message
        buffer.Assert(false, false, freeSpace - HeaderSize - messageCBytes, Array.Empty<byte>());

        // write messageC left body
        while (messageCBytes < messageC.Length)
        {
            if (buffer.IsFull)
                buffer.Grow();

            var chunkSize = Math.Min(messageC.Length - messageCBytes, buffer.FreeSpace.Length);
            buffer.Write(messageC.AsSpan(messageCBytes, chunkSize));

            messageCBytes += chunkSize;
        }

        // assert - free space is available, messageC is detected
        var freeSpaceBeforeReset = buffer.FreeSpace.Length;
        buffer.Assert(false, true, freeSpaceBeforeReset, messageC);

        // reset - drops messageC and resets buffer
        buffer.Reset();

        // assert
        var newFreeSpace = buffer.FreeSpace.Length;
        buffer.Assert(false, false, newFreeSpace, Array.Empty<byte>());
        newFreeSpace.IsGreater(freeSpaceBeforeReset);
        newFreeSpace.IsGreater(freeSpace);
    }
}

file static class BufferExtensions
{
    public static void WriteMessage(this MessagingBuffer buffer, byte[] data)
    {
        buffer.WriteMessageSize(data.Length);
        buffer.Write(data);
    }

    public static void WriteMessageSize(this MessagingBuffer buffer, int size)
    {
        buffer.Write(BitConverter.GetBytes(size));
    }

    public static void Write(this MessagingBuffer buffer, ReadOnlySpan<byte> data)
    {
        var freeSpace = buffer.FreeSpace.Span;
        for (int i = 0; i < data.Length; i++)
            freeSpace[i] = data[i];

        buffer.TrackData(data.Length);
    }

    public static void Assert(
        this MessagingBuffer buffer,
        bool isFull,
        bool containsFullMessage,
        int freeSpace,
        byte[] message
    )
    {
        buffer.IsFull.Is(isFull);
        buffer.ContainsFullMessage.Is(containsFullMessage);
        buffer.FreeSpace.Length.Is(freeSpace);

        var bufferMessage = buffer.Message.ToArray();
        bufferMessage.IsEqual(message);
    }
}
