using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Numerics;

namespace Base2N.Decoders;

public class PipeBase2NDecoder(PipeWriter writer, int radix, bool leaveOpen = false)
    : BufferWriterBase2NDecoder(writer, radix), IBase2NAsyncDecoder
{
    private byte[]? _asyncBuffer;
    private readonly bool _leaveOpen = leaveOpen;

    public PipeBase2NDecoder(Stream data, int radix, StreamPipeWriterOptions? options = null)
        : this(PipeWriter.Create(data, options), radix, false) { }

    public async ValueTask WriteDigitAsync(int digit, int extraBits = 0, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_currentBits < 0, typeof(PipeBase2NDecoder));
        int mask = Radix - 1;
        int bitsPerDigit = BitOperations.PopCount((uint)mask);
        if (_currentBits > 64 - bitsPerDigit)
            await FlushAsync(cancellationToken).ConfigureAwait(false);
        _currentBits += bitsPerDigit - extraBits;
        _buffer = ((_buffer << bitsPerDigit) | (uint)(digit & mask)) >> extraBits;
    }
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_currentBits < 0, typeof(PipeBase2NDecoder));
        (int byteCount, int remainingBits) = Math.DivRem(_currentBits, 8);
        ulong output = _buffer << (64 - _currentBits);
        BinaryPrimitives.WriteUInt64BigEndian(_asyncBuffer ??= new byte[8], output);
        await ((PipeWriter)Writer).WriteAsync(_asyncBuffer.AsMemory(0, byteCount), cancellationToken).ConfigureAwait(false);
        _currentBits = remainingBits;
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!_leaveOpen)
            ((PipeWriter)Writer).Complete();
    }
    public async ValueTask DisposeAsync()
    {
        if (_currentBits >= 0)
        {
            await FlushAsync().ConfigureAwait(false);
            _currentBits = -1;
            if (!_leaveOpen)
                await ((PipeWriter)Writer).CompleteAsync().ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
}