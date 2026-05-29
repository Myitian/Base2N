using Base2N.Streams;
using System.Buffers;
using System.Text;

namespace Base2N.Text.Streams;

public sealed class TextWriterStream : WriteOnlyStream
{
    private readonly Decoder _decoder;
    private readonly char[] _charBuffer;
    private State _state;
    public TextWriter BaseWriter { get; }

    public TextWriterStream(
        TextWriter textWriter,
        Encoding? encoding = null,
        int charBufferSize = 0,
        bool pooledBuffer = false,
        bool clearPooledBuffer = false,
        bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(textWriter);
        ArgumentOutOfRangeException.ThrowIfNegative(charBufferSize);
        encoding ??= Encoding.UTF8;
        _decoder = encoding.GetDecoder();
        BaseWriter = textWriter;
        _state = (leaveOpen ? State.LeaveOpen : 0)
            | (pooledBuffer ? State.PooledBuffer : 0)
            | (clearPooledBuffer ? State.ClearPooledBuffer : 0);
        charBufferSize = Math.Max(charBufferSize == 0 ? 1024 : charBufferSize, encoding.GetMaxCharCount(1));
        _charBuffer = pooledBuffer ?
            ArrayPool<char>.Shared.Rent(charBufferSize) :
            GC.AllocateUninitializedArray<char>(charBufferSize);
    }
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(_state.HasFlag(State.Disposed), this);
        while (!buffer.IsEmpty)
        {
            _decoder.Convert(buffer, _charBuffer, false, out int bytesUsed, out int charsUsed, out _);
            buffer = buffer[bytesUsed..];
            BaseWriter.Write(_charBuffer, 0, charsUsed);
        }
    }
    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));
    public override void WriteByte(byte value)
        => Write(new ReadOnlySpan<byte>(in value));
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_state.HasFlag(State.Disposed), this);
        while (!buffer.IsEmpty)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _decoder.Convert(buffer.Span, _charBuffer, false, out int bytesUsed, out int charsUsed, out _);
            buffer = buffer[bytesUsed..];
            await BaseWriter.WriteAsync(_charBuffer.AsMemory(0, charsUsed), cancellationToken)
                .ConfigureAwait(false);
        }
    }
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
    public override void Flush()
    {
        bool completed;
        do
        {
            _decoder.Convert([], _charBuffer, true, out _, out int charsUsed, out completed);
            BaseWriter.Write(_charBuffer, 0, charsUsed);
        }
        while (!completed);
    }
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        bool completed;
        do
        {
            _decoder.Convert([], _charBuffer, true, out _, out int charsUsed, out completed);
            await BaseWriter.WriteAsync(_charBuffer.AsMemory(0, charsUsed), cancellationToken)
                .ConfigureAwait(false);
        }
        while (!completed);
    }
#pragma warning disable CA2215
    protected override void Dispose(bool disposing)
    {
        if (!_state.HasFlag(State.Disposed))
        {
            _state |= State.Disposed;
            if (_state.HasFlag(State.PooledBuffer))
                ArrayPool<char>.Shared.Return(_charBuffer, _state.HasFlag(State.ClearPooledBuffer));
            if (!_state.HasFlag(State.LeaveOpen))
                BaseWriter.Dispose();
        }
    }
    public override async ValueTask DisposeAsync()
    {
        if (!_state.HasFlag(State.Disposed))
        {
            _state |= State.Disposed;
            if (_state.HasFlag(State.PooledBuffer))
                ArrayPool<char>.Shared.Return(_charBuffer, _state.HasFlag(State.ClearPooledBuffer));
            if (!_state.HasFlag(State.LeaveOpen))
                await BaseWriter.DisposeAsync().ConfigureAwait(false);
        }
    }
}
