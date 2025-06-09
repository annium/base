using System;
using System.Buffers;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// A buffer implementation for messaging socket operations with frame length prefixes
/// </summary>
internal class MessagingBuffer : IDisposable
{
    /// <summary>
    /// The size of the message header in bytes (4 bytes for int32 length)
    /// </summary>
    public const int HeaderSize = sizeof(int);

    /// <summary>
    /// Wrap buffer free space as Memory
    /// </summary>
    /// <returns>Buffer free space as Memory</returns>
    public Memory<byte> FreeSpace
    {
        get
        {
            EnsureNotDisposed();

            return new Memory<byte>(_buffer, DataEnd, _buffer.Length - DataEnd);
        }
    }

    /// <summary>
    /// Returns whether buffer is full
    /// </summary>
    /// <returns>Whether buffer is full</returns>
    public bool IsFull => _buffer.Length == DataEnd;

    /// <summary>
    /// Returns whether buffer contains full message
    /// </summary>
    /// <returns>Whether buffer contains full message</returns>
    public bool ContainsFullMessage
    {
        get
        {
            EnsureNotDisposed();

            return _dataLength >= HeaderSize + _messageSize;
        }
    }

    /// <summary>
    /// Returns whether buffer contains full message
    /// </summary>
    /// <returns>Whether buffer contains full message</returns>
    public bool ExtremeMessageExpected
    {
        get
        {
            EnsureNotDisposed();

            return _messageSize >= _maxMessageSize;
        }
    }

    /// <summary>
    /// Wrap buffer message as ReadOnlyMemory
    /// </summary>
    /// <returns>Buffer data as ReadOnlyMemory</returns>
    public ReadOnlyMemory<byte> Message
    {
        get
        {
            EnsureNotDisposed();

            return ContainsFullMessage
                ? new ReadOnlyMemory<byte>(_buffer, _dataStart + HeaderSize, _messageSize)
                : new ReadOnlyMemory<byte>(Array.Empty<byte>());
        }
    }

    /// <summary>
    /// Gets the end position of data in the buffer
    /// </summary>
    private int DataEnd => _dataStart + _dataLength;

    /// <summary>
    /// The maximum allowed message size for extreme message detection
    /// </summary>
    private readonly int _maxMessageSize;

    /// <summary>
    /// The starting position of data in the buffer
    /// </summary>
    private int _dataStart;

    /// <summary>
    /// The length of data currently in the buffer
    /// </summary>
    private int _dataLength;

    /// <summary>
    /// The size of the current message being processed
    /// </summary>
    private int _messageSize;

    /// <summary>
    /// The underlying byte array buffer from the array pool
    /// </summary>
    private byte[] _buffer;

    /// <summary>
    /// Flag indicating whether the buffer has been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the MessagingBuffer class
    /// </summary>
    /// <param name="size">The buffer size</param>
    /// <param name="maxMessageSize">The maximum expected message size</param>
    public MessagingBuffer(int size, int maxMessageSize)
    {
        _maxMessageSize = maxMessageSize;
        _buffer = ArrayPool<byte>.Shared.Rent(HeaderSize + size);
    }

    /// <summary>
    /// Reset buffer internal data length to 0, allowing write from start
    /// </summary>
    public void Reset()
    {
        EnsureNotDisposed();

        // if no data - simply reset state
        if (_dataLength == 0)
        {
            _dataStart = 0;
            _dataLength = 0;
            _messageSize = 0;
            return;
        }

        // if contains full message - move pointer, update state
        if (ContainsFullMessage)
        {
            var fullMessageSize = HeaderSize + _messageSize;
            _dataLength -= fullMessageSize;
            if (_dataLength > 0)
                _dataStart += fullMessageSize;
            else
                _dataStart = 0;
            _messageSize = 0;
            TryResolveMessageSize();

            // if no data at all or still contains full message - no action
            if (_dataLength == 0 || ContainsFullMessage)
                return;
        }

        // if buffer contains some data, but not a complete message - move data to start in buffer and adjust pointers
        _buffer.AsSpan(_dataStart, _dataLength).CopyTo(_buffer);
        _dataStart = 0;
        _messageSize = 0;
        TryResolveMessageSize();
    }

    /// <summary>
    /// Track data size, written to buffer
    /// </summary>
    /// <param name="dataSize">
    /// count of elements, written to buffer
    /// </param>
    public void TrackData(int dataSize)
    {
        _dataLength += dataSize;
        TryResolveMessageSize();
    }

    /// <summary>
    /// Resize buffer up due to not enough capacity and track count of elements, written to buffer
    /// </summary>
    public void Grow()
    {
        EnsureNotDisposed();

        // create new buffer x2
        var newBuffer = ArrayPool<byte>.Shared.Rent(_buffer.Length * 2);

        // copy from current buffer to new one (resetting to the start)
        _buffer.AsSpan(_dataStart, _dataLength).CopyTo(newBuffer);

        // return old buffer to pool
        ArrayPool<byte>.Shared.Return(_buffer);

        // replace old buffer with new
        _buffer = newBuffer;

        // reset data start to 0
        _dataStart = 0;
    }

    /// <summary>
    /// Disposes the buffer and returns it to the array pool
    /// </summary>
    public void Dispose()
    {
        EnsureNotDisposed();

        _isDisposed = true;
        ArrayPool<byte>.Shared.Return(_buffer);
    }

    /// <summary>
    /// Returns a string representation of the buffer state
    /// </summary>
    /// <returns>A string describing the buffer state</returns>
    public override string ToString() =>
        $"{_dataStart}->{_dataLength} [{(_messageSize > 0 ? HeaderSize + _messageSize : 0)} | {_buffer.Length}] {{max:{_maxMessageSize}}}";

    /// <summary>
    /// Attempts to resolve the message size from the header if not already resolved
    /// </summary>
    private void TryResolveMessageSize()
    {
        if (_messageSize == 0 && _dataLength >= HeaderSize)
            _messageSize = BitConverter.ToInt32(new ReadOnlySpan<byte>(_buffer, _dataStart, HeaderSize));
    }

    /// <summary>
    /// Ensures the buffer has not been disposed
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer is disposed</exception>
    private void EnsureNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("Buffer is already disposed");
        }
    }
}
