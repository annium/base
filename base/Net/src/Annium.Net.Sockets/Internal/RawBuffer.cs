using System;
using System.Buffers;

namespace Annium.Net.Sockets.Internal;

internal struct RawBuffer : IDisposable
{
    private readonly byte[] _buffer;
    private int _dataLength;
    private bool _isDisposed;

    public RawBuffer(int size)
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
    /// <param name="dataSize">
    /// count of elements, written to buffer
    /// </param>
    public void TrackData(int dataSize)
    {
        _dataLength += dataSize;
    }

    /// <summary>
    /// Wrap buffer free space as ArraySegment
    /// </summary>
    /// <returns>Buffer free space as ArraySegment</returns>
    public Memory<byte> AsFreeSpaceMemory()
    {
        EnsureNotDisposed();

        return new Memory<byte>(_buffer, _dataLength, _buffer.Length - _dataLength);
    }

    /// <summary>
    /// Wrap buffer data as ReadOnlySpan
    /// </summary>
    /// <returns>Buffer data as ReadOnlySpan</returns>
    public ReadOnlyMemory<byte> AsDataReadOnlyMemory()
    {
        EnsureNotDisposed();

        return new ReadOnlyMemory<byte>(_buffer, 0, _dataLength);
    }

    public void Dispose()
    {
        EnsureNotDisposed();

        _isDisposed = true;
        ArrayPool<byte>.Shared.Return(_buffer);
    }

    private void EnsureNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException("Buffer is already disposed");
        }
    }
}