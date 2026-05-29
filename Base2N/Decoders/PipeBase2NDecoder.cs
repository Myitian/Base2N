using System.Buffers.Binary;
using System.IO.Pipelines;

namespace Base2N.Decoders;

public class PipeBase2NDecoder(PipeWriter writer, int radix, bool leaveOpen = false)
    : BufferWriterBase2NDecoder(writer, radix), IAsyncBase2NDecoder
{
    private byte[]? _asyncBuffer;
    private readonly bool _leaveOpen = leaveOpen;

    public PipeBase2NDecoder(Stream data, int radix, StreamPipeWriterOptions? options = null)
        : this(PipeWriter.Create(data, options), radix, false) { }

    public ValueTask WriteDigitAsync(int digit, int extraBits = -1, CancellationToken cancellationToken = default)
        => cancellationToken.IsCancellationRequested ?
            ValueTask.FromCanceled(cancellationToken) :
            WriteDigitAsync(this, digit, extraBits, cancellationToken);
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(CurrentBits < 0, this);
        (int byteCount, int remainingBits) = Math.DivRem(CurrentBits, 8);
        ulong output = Buffer << (64 - CurrentBits);
        BinaryPrimitives.WriteUInt64BigEndian(_asyncBuffer ??= new byte[8], output);
        await ((PipeWriter)Writer).WriteAsync(_asyncBuffer.AsMemory(0, byteCount), cancellationToken).ConfigureAwait(false);
        CurrentBits = remainingBits;
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!_leaveOpen)
            ((PipeWriter)Writer).Complete();
    }
    public async ValueTask DisposeAsync()
    {
        if (CurrentBits >= 0)
        {
            await FlushAsync().ConfigureAwait(false);
            CurrentBits = -1;
            if (!_leaveOpen)
                await ((PipeWriter)Writer).CompleteAsync().ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
}