using Base2N.Streams;
using System.Buffers;
using System.Text;

namespace Base2N.Text.Streams;

public sealed class TextReaderStream : ReadOnlyStream
{
    private readonly Encoder _encoder;
    private readonly byte[] _byteBuffer;
    private readonly char[] _charBuffer;
    private int _byteBufferOffset;
    private int _byteBufferCount;
    private int _charBufferOffset;
    private int _charBufferCount;
    private State _state;
    public TextReader BaseReader { get; }
    public bool IsCompleted
        => _state.HasFlag(State.EncoderCompleted) && _byteBufferCount == 0;

    public TextReaderStream(
        TextReader textReader,
        Encoding? encoding = null,
        int charBufferSize = 0,
        bool pooledBuffer = false,
        bool clearPooledBuffer = false,
        bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(textReader);
        ArgumentOutOfRangeException.ThrowIfNegative(charBufferSize);
        encoding ??= Encoding.UTF8;
        _encoder = encoding.GetEncoder();
        BaseReader = textReader;
        _state = (leaveOpen ? State.LeaveOpen : 0)
            | (pooledBuffer ? State.PooledBuffer : 0)
            | (clearPooledBuffer ? State.ClearPooledBuffer : 0);
        _byteBuffer = new byte[encoding.GetMaxByteCount(1)]; // The buffer usually smaller than 16 bytes, so no need to pool it.
        charBufferSize = charBufferSize == 0 ? 1024 : charBufferSize;
        _charBuffer = pooledBuffer ?
            ArrayPool<char>.Shared.Rent(charBufferSize) :
            GC.AllocateUninitializedArray<char>(charBufferSize);
    }
    public override int Read(Span<byte> buffer)
    {
        State state = _state;
        ObjectDisposedException.ThrowIf(state.HasFlag(State.Disposed), this);
        int byteCount = _byteBufferCount;
        int charCount = _charBufferCount;
        if (IsCompleted)
            return 0;
        int result;
        int byteOffset = _byteBufferOffset;
        if (byteCount == 0)
        {
            int charOffset = _charBufferOffset;
            bool useDirectBuffer = buffer.Length >= _byteBuffer.Length;
            while (byteCount == 0)
            {
                bool flush = state.HasFlag(State.ReaderCompleted);
                if (charCount == 0 && !flush)
                {
                    int read = BaseReader.Read(_charBuffer.AsSpan(charOffset + charCount));
                    charCount += read;
                    if (read == 0)
                    {
                        state |= State.ReaderCompleted;
                        flush = true;
                    }
                }
                _encoder.Convert(
                    _charBuffer.AsSpan(charOffset, charCount),
                    useDirectBuffer ? buffer : _byteBuffer,
                    flush,
                    out int charsUsed,
                    out byteCount,
                    out bool completed);
                if (flush && completed)
                {
                    state |= State.EncoderCompleted;
                    break;
                }
                charCount -= charsUsed;
                charOffset = charCount == 0 ? 0 : charOffset + charsUsed;
            }
            _charBufferOffset = charOffset;
            _charBufferCount = charCount;
            byteOffset = 0;
            if (useDirectBuffer)
            {
                result = byteCount;
                goto EXIT;
            }
        }
        result = Math.Min(byteCount, buffer.Length);
        _byteBuffer.AsSpan(byteOffset, result).CopyTo(buffer);
        _byteBufferOffset = byteOffset + result;
        _byteBufferCount = byteCount - result;
    EXIT:
        _state = state;
        return result;
    }
    public override int ReadByte()
    {
        byte result = default;
        return Read(new Span<byte>(ref result)) == 0 ? -1 : result;
    }
    public override int Read(byte[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State state = _state;
        ObjectDisposedException.ThrowIf(state.HasFlag(State.Disposed), this);
        if (IsCompleted)
            return 0;
        int byteCount = _byteBufferCount;
        int charCount = _charBufferCount;
        int result;
        int byteOffset = _byteBufferOffset;
        if (byteCount == 0)
        {
            int charOffset = _charBufferOffset;
            bool useDirectBuffer = buffer.Length >= _byteBuffer.Length;
            while (byteCount == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                bool flush = state.HasFlag(State.ReaderCompleted);
                if (charCount == 0 && !flush)
                {
                    int read = await BaseReader.ReadAsync(_charBuffer.AsMemory(charOffset + charCount), cancellationToken)
                        .ConfigureAwait(false);
                    charCount += read;
                    if (read == 0)
                    {
                        state |= State.ReaderCompleted;
                        flush = true;
                    }
                }
                _encoder.Convert(
                    _charBuffer.AsSpan(charOffset, charCount),
                    useDirectBuffer ? buffer.Span : _byteBuffer,
                    flush,
                    out int charsUsed,
                    out byteCount,
                    out bool completed);
                if (flush && completed)
                {
                    state |= State.EncoderCompleted;
                    break;
                }
                charCount -= charsUsed;
                charOffset = charCount == 0 ? 0 : charOffset + charsUsed;
            }
            _charBufferOffset = charOffset;
            _charBufferCount = charCount;
            byteOffset = 0;
            if (useDirectBuffer)
            {
                result = byteCount;
                goto EXIT;
            }
        }
        result = Math.Min(byteCount, buffer.Length);
        _byteBuffer.AsSpan(byteOffset, result).CopyTo(buffer.Span);
        _byteBufferOffset = byteOffset + result;
        _byteBufferCount = byteCount - result;
    EXIT:
        _state = state;
        return result;
    }
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
#pragma warning disable CA2215
    protected override void Dispose(bool disposing)
    {
        if (!_state.HasFlag(State.Disposed))
        {
            _state |= State.Disposed;
            if (_state.HasFlag(State.PooledBuffer))
                ArrayPool<char>.Shared.Return(_charBuffer, _state.HasFlag(State.ClearPooledBuffer));
            if (!_state.HasFlag(State.LeaveOpen))
                BaseReader.Dispose();
        }
    }
}