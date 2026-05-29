using Base2N.Encoders;
using System.Numerics;

namespace Base2N.Streams;

public sealed class AsyncBase2NDecoderStream<TBase2NDigitReader>(TBase2NDigitReader reader, int radix, bool leaveOpen = false)
    : Base2NDecoderStream<TBase2NDigitReader>(reader, radix, leaveOpen)
    where TBase2NDigitReader : IBase2NDigitReader, IAsyncBase2NDigitReader
{
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => await ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        if (ReadCompleted || buffer.IsEmpty)
            return 0;
        int mask = Radix - 1;
        int bitPerDigit = BitOperations.PopCount((uint)mask);
        ulong bitBuffer = BitBuffer;
        int currentBits = CurrentBits;
        int minRequiredBits = Math.Max(bitPerDigit, 8);
        while (currentBits <= minRequiredBits)
        {
            bool hasNext = await BaseReader.MoveNextAsync().ConfigureAwait(false);
            if (!hasNext)
            {
                ReadCompleted = true;
                int offset = -BaseReader.CurrentBitCount;
                bitBuffer >>= offset;
                currentBits -= offset;
                break;
            }
            bitBuffer = (bitBuffer << bitPerDigit) | (uint)GetCurrent(BaseReader);
            currentBits += bitPerDigit;
        }
        int i = 0;
        while ((currentBits -= 8) >= 0 && i < buffer.Length)
            buffer.Span[i++] = (byte)(bitBuffer >> currentBits);
        BitBuffer = bitBuffer;
        CurrentBits = currentBits + 8;
        return i;

        static int GetCurrent<TEnumerator>(TEnumerator enumerator) where TEnumerator : IAsyncEnumerator<int>
            => enumerator.Current;
    }
#pragma warning disable CA2215
    public override async ValueTask DisposeAsync()
    {
        if (!Disposed)
        {
            Disposed = true;
            if (!LeaveOpen)
                await BaseReader.DisposeAsync().ConfigureAwait(false);
        }
    }
}