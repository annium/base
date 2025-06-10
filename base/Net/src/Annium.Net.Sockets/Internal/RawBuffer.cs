using System;
using System.Buffers;

namespace Annium.Net.Sockets.Internal;

/// <summary>
/// A buffer implementation for raw socket operations that uses pooled arrays
/// </summary>
internal struct RawBuffer : IDisposable
{
    /// <summary>
    /// The underlying byte buffer from the array pool
    /// </summary>
    private readonly byte[] _buffer;

    /// <summary>
    /// The current length of valid data in the buffer
    /// </summary>
    private int _dataLength;

    /// <summary>
    /// Indicates whether the buffer has been disposed
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the RawBuffer struct
    /// </summary>
    /// <param name="size">The minimum size of the buffer to rent from the array pool</param>
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
    /// Disposes the buffer and returns it to the array pool
    /// </summary>
    public void Dispose()
    {
        EnsureNotDisposed();

        _isDisposed = true;
        ArrayPool<byte>.Shared.Return(_buffer);
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
