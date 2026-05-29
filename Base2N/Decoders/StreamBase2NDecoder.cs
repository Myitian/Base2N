using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Base2N.Decoders;

public class StreamBase2NDecoder(Stream stream, int radix, bool leaveOpen = false) : AbstractBase2NDecoder(radix), IAsyncBase2NDecoder
{
    private byte[]? _asyncBuffer;
    private readonly bool _leaveOpen = leaveOpen;

    public Stream BaseStream { get; } = stream;

    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(CurrentBits < 0, this);
        (int byteCount, int remainingBits) = Math.DivRem(CurrentBits, 8);
        ulong output = Buffer << (64 - CurrentBits);
        if (BitConverter.IsLittleEndian)
            output = BinaryPrimitives.ReverseEndianness(output);
        BaseStream.Write(MemoryMarshal.AsBytes(new ReadOnlySpan<ulong>(ref output))[..byteCount]);
        CurrentBits = remainingBits;
    }
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
        await BaseStream.WriteAsync(_asyncBuffer.AsMemory(0, byteCount), cancellationToken).ConfigureAwait(false);
        CurrentBits = remainingBits;
    }
    protected override void Dispose(bool disposing)
    {
        if (CurrentBits >= 0)
        {
            Flush();
            CurrentBits = -1;
            if (!_leaveOpen)
                BaseStream.Dispose();
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (CurrentBits >= 0)
        {
            await FlushAsync().ConfigureAwait(false);
            CurrentBits = -1;
            if (!_leaveOpen)
                await BaseStream.DisposeAsync().ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
}