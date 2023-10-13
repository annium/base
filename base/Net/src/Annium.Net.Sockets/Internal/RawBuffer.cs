using System;
using System.Buffers;

namespace Annium.Net.Sockets.Internal;

internal struct RawBuffer : IDisposable
{
    private readonly byte[] _buffer;
    private int _dataLength;
    private bool _isDisposed;

    /// <summary>
    /// Wrap buffer free space as Memory
    /// </summary>
    /// <returns>Buffer free space as Memory</returns>
    public Memory<byte> FreeSpace
    {
        get
        {
            EnsureNotDisposed();

            return new Memory<byte>(_buffer, _dataLength, _buffer.Length - _dataLength);
        }
    }

    /// <summary>
    /// Wrap buffer message as ReadOnlyMemory
    /// </summary>
    /// <returns>Buffer data as ReadOnlyMemory</returns>
    public ReadOnlyMemory<byte> Data
    {
        get
        {
            EnsureNotDisposed();

            return new ReadOnlyMemory<byte>(_buffer, 0, _dataLength);
        }
    }

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