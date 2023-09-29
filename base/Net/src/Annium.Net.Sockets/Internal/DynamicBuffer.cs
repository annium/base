using System;
using System.Buffers;

namespace Annium.Net.Sockets.Internal;

internal struct DynamicBuffer<T> : IDisposable
    where T : struct
{
    private T[] _buffer;
    private int _dataLength;
    private bool _isDisposed;

    public DynamicBuffer(int size)
    {
        _buffer = ArrayPool<T>.Shared.Rent(size);
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
    /// <param name="dataSize">
    /// count of elements, written to buffer
    /// </param>
    public void TrackDataSize(int dataSize)
    {
        _dataLength += dataSize;
    }

    /// <summary>
    /// Resize buffer up due to not enough capacity and track count of elements, written to buffer
    /// </summary>
    public void Grow()
    {
        EnsureNotDisposed();
        // create new buffer x2
        var newBuffer = ArrayPool<T>.Shared.Rent(_buffer.Length * 2);

        // copy from current buffer to new one
        _buffer.CopyTo(newBuffer, 0);

        // return old buffer to pool
        ArrayPool<T>.Shared.Return(_buffer);

        // replace old buffer with new
        _buffer = newBuffer;
    }

    /// <summary>
    /// Wrap buffer free space as ArraySegment
    /// </summary>
    /// <returns>Buffer free space as ArraySegment</returns>
    public Memory<T> AsFreeSpaceMemory()
    {
        EnsureNotDisposed();

        return new Memory<T>(_buffer, _dataLength, _buffer.Length - _dataLength);
    }

    /// <summary>
    /// Wrap buffer data as ReadOnlySpan
    /// </summary>
    /// <returns>Buffer data as ReadOnlySpan</returns>
    public ReadOnlyMemory<T> AsDataReadOnlyMemory()
    {
        EnsureNotDisposed();

        return new ReadOnlyMemory<T>(_buffer, 0, _dataLength);
    }

    public void Dispose()
    {
        EnsureNotDisposed();

        _isDisposed = true;
        ArrayPool<T>.Shared.Return(_buffer);
    }

    private void EnsureNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("Buffer is already disposed");
        }
    }
}