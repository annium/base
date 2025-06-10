using System;
using System.Buffers;

namespace Annium.Net.WebSockets.Internal;

/// <summary>
/// Internal managed buffer for WebSocket message receiving operations using array pooling
/// </summary>
internal struct ManagedBuffer : IDisposable
{
    /// <summary>
    /// The underlying buffer array from the array pool
    /// </summary>
    private byte[] _buffer;

    /// <summary>
    /// The current length of valid data in the buffer
    /// </summary>
    private int _dataLength;

    /// <summary>
    /// Indicates whether the buffer has been disposed
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Wrap buffer free space as ArraySegment
    /// </summary>
    /// <returns>Buffer free space as ArraySegment</returns>
    public Memory<byte> FreeSpace
    {
        get
        {
            EnsureNotDisposed();

            return new Memory<byte>(_buffer, _dataLength, _buffer.Length - _dataLength);
        }
    }

    /// <summary>
    /// Wrap buffer data as ReadOnlySpan
    /// </summary>
    /// <returns>Buffer data as ReadOnlySpan</returns>
    public ReadOnlyMemory<byte> Data
    {
        get
        {
            EnsureNotDisposed();

            return new ReadOnlyMemory<byte>(_buffer, 0, _dataLength);
        }
    }

    /// <summary>
    /// Initializes a new instance of the ManagedBuffer structure with the specified initial size
    /// </summary>
    /// <param name="size">The initial size of the buffer</param>
    public ManagedBuffer(int size)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(size);
        _dataLength = 0;
        _isDisposed = false;
    }

    /// <summary>
    /// Reset buffer internal data length to 0, allowing write from start
    /// </summary>
    public void Reset()
    {
        EnsureNotDisposed();
        _dataLength = 0;
    }

    /// <summary>
    /// Track data size, written to buffer
    /// </summary>
    /// <param name="dataSize">Count of elements written to buffer</param>
    public void TrackDataSize(int dataSize)
    {
        _dataLength += dataSize;
    }

    /// <summary>
    /// Resize buffer up due to not enough capacity and track count of elements, written to buffer
    /// </summary>
    /// <summary>
    /// Resize buffer up due to not enough capacity and track count of elements, written to buffer
    /// </summary>
    public void Grow()
    {
        EnsureNotDisposed();
        // create new buffer x2
        var newBuffer = ArrayPool<byte>.Shared.Rent(_buffer.Length * 2);

        // copy from current buffer to new one
        _buffer.CopyTo(newBuffer, 0);

        // return old buffer to pool
        ArrayPool<byte>.Shared.Return(_buffer);

        // replace old buffer with new
        _buffer = newBuffer;
    }

    /// <summary>
    /// Disposes the managed buffer and returns the underlying array to the pool
    /// </summary>
    public void Dispose()
    {
        EnsureNotDisposed();

        _isDisposed = true;
        ArrayPool<byte>.Shared.Return(_buffer);
    }

    /// <summary>
    /// Ensures the buffer has not been disposed, throws exception if it has
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the buffer is already disposed</exception>
    private void EnsureNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("Buffer is already disposed");
        }
    }
}
