using Base2N.Streams;
using System.Text;

namespace Base2N.Text.Streams;

public sealed class StringReaderStream : ReadOnlyStream
{
    private readonly Encoder _encoder;
    private readonly byte[] _byteBuffer;
    private int _byteBufferOffset;
    private int _byteBufferCount;
    private int _charBufferOffset;
    private State _state;
    public ReadOnlyMemory<char> Text { get; }
    public bool IsCompleted
        => _state.HasFlag(State.EncoderCompleted) && _byteBufferCount == 0;

    public StringReaderStream(string text, Encoding? encoding = null) : this(text.AsMemory(), encoding) { }
    public StringReaderStream(ReadOnlyMemory<char> text, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        _encoder = encoding.GetEncoder();
        Text = text;
        _byteBuffer = new byte[encoding.GetMaxByteCount(1)];
    }
    public override int Read(Span<byte> buffer)
    {
        State state = _state;
        ObjectDisposedException.ThrowIf(state.HasFlag(State.Disposed), this);
        int byteCount = _byteBufferCount;
        if (IsCompleted)
            return 0;
        int result;
        int byteOffset = _byteBufferOffset;
        if (byteCount == 0)
        {
            int charOffset = _charBufferOffset;
            bool useDirectBuffer = buffer.Length >= _byteBuffer.Length;
            _encoder.Convert(
                Text.Span[charOffset..],
                useDirectBuffer ? buffer : _byteBuffer,
                true,
                out int charsUsed,
                out byteCount,
                out bool completed);
            if (completed)
                state |= State.EncoderCompleted;
            charOffset += charsUsed;
            _charBufferOffset = charOffset;
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
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled<int>(cancellationToken) :
            ValueTask.FromResult(Read(buffer.Span));
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        => cancellationToken.IsCancellationRequested ?
            Task.FromCanceled<int>(cancellationToken) :
            Task.FromResult(Read(buffer.AsSpan(offset, count)));

#pragma warning disable CA2215
    protected override void Dispose(bool disposing)
        => _state |= State.Disposed;
}