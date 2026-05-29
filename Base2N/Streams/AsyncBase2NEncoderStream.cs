using Base2N.Decoders;
using System.Numerics;

namespace Base2N.Streams;

public sealed class AsyncBase2NEncoderStream<TBase2NDigitWriter>(TBase2NDigitWriter writer, int radix, bool leaveOpen = false)
    : Base2NEncoderStream<TBase2NDigitWriter>(writer, radix, leaveOpen)
    where TBase2NDigitWriter : IBase2NDigitWriter, IAsyncBase2NDigitWriter
{
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await WriteAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        int mask = Radix - 1;
        int bitPerDigit = BitOperations.PopCount((uint)mask);
        ulong bitBuffer = BitBuffer;
        int currentBits = CurrentBits;
        for (int i = 0; i < buffer.Length; i++)
        {
            bitBuffer = (bitBuffer << 8) | buffer.Span[i];
            currentBits += 8;
            while ((currentBits -= bitPerDigit) > 0)
                await BaseWriter.WriteDigitAsync((int)(bitBuffer >> currentBits) & mask, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            currentBits += bitPerDigit;
        }
        BitBuffer = bitBuffer;
        CurrentBits = currentBits;
    }
    public override Task FlushAsync(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested ?
            Task.FromCanceled(cancellationToken) :
            Task.CompletedTask;
    public async ValueTask CompleteAsync(CancellationToken cancellationToken = default)
    {
        int mask = Radix - 1;
        int bitPerDigit = BitOperations.PopCount((uint)mask);
        ulong buffer = BitBuffer;
        int currentBits = CurrentBits;
        if (currentBits != 0)
        {
            await BaseWriter.WriteDigitAsync(0, int.MaxValue, cancellationToken)
                .ConfigureAwait(false);
            return;
        }
        while ((currentBits -= bitPerDigit) > 0)
            await BaseWriter.WriteDigitAsync((int)(buffer >> currentBits) & mask, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        int extraBits = -currentBits;
        await BaseWriter.WriteDigitAsync((int)(buffer << extraBits) & mask, extraBits, cancellationToken)
            .ConfigureAwait(false);
        CurrentBits = 0;
    }
#pragma warning disable CA2215
    public override async ValueTask DisposeAsync()
    {
        if (!Disposed)
        {
            Disposed = true;
            await CompleteAsync().ConfigureAwait(false);
            if (!LeaveOpen)
                await BaseWriter.DisposeAsync().ConfigureAwait(false);
        }
    }
}