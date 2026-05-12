using System.Buffers.Binary;
using System.Numerics;

namespace Base2N;

public static class Base2NUtils
{
    public static int DigitAt(ReadOnlySpan<byte> data, int radix, int index)
    {
        int mask = radix - 1;
        int bitsPerDigit = BitOperations.PopCount((uint)mask);
        ulong bitOffset = (ulong)index * (ulong)bitsPerDigit;
        int byteOffset = (int)(bitOffset / 8);
        int bitInByteOffset = (int)bitOffset % 8;
        int bytesToRead = (bitsPerDigit + bitInByteOffset + 7) / 8;
        ReadOnlySpan<byte> source = data.Slice(byteOffset, Math.Min(bytesToRead, data.Length - byteOffset));
        Span<byte> buffer = stackalloc byte[8];
        buffer.Clear();
        source.CopyTo(buffer);
        return (int)(BinaryPrimitives.ReadUInt64BigEndian(buffer) >> (64 - bitsPerDigit - bitInByteOffset)) & mask;
    }
    public static bool MoveNext<T>(ref T enumerator)
        where T : IBase2NEnumerator, allows ref struct
    {
        int bitsPerDigit = BitOperations.PopCount((uint)enumerator.Mask);
        int offset = enumerator.CurrentBits - bitsPerDigit;
        if (offset < 0)
        {
            if (enumerator.ReadingCompleted)
            {
                if (enumerator.CurrentBits > 0)
                {
                    enumerator.Buffer <<= bitsPerDigit - enumerator.CurrentBits;
                    enumerator.Current = (int)enumerator.Buffer & enumerator.Mask;
                    enumerator.CurrentBits -= bitsPerDigit;
                    // Negative value indicates the extra bits in output sequence
                    return true;
                }
                return false;
            }
            int bytesToRead = ~offset / 8 + 1;
            int bitsToRead = bytesToRead * 8;
            uint dataRead = enumerator.ReadAtLeast(ref bytesToRead) >> (32 - bitsToRead);
            if (enumerator.CurrentBits == 0 && bytesToRead == 0)
                return false;
            enumerator.Buffer = (enumerator.Buffer << bitsToRead) | dataRead;
            enumerator.CurrentBits += bitsToRead;
            offset = enumerator.CurrentBits - bitsPerDigit;
            int bitsReadDiff = bitsToRead - bytesToRead * 8;
            if (bitsReadDiff > 0)
                enumerator.CurrentBits -= bitsReadDiff;
        }
        enumerator.Current = (int)(enumerator.Buffer >> offset) & enumerator.Mask;
        enumerator.CurrentBits -= bitsPerDigit;
        return true;
    }
    public static bool MoveNext(IBase2NEnumerator enumerator)
    {
        int bitsPerDigit = BitOperations.PopCount((uint)enumerator.Mask);
        int offset = enumerator.CurrentBits - bitsPerDigit;
        if (offset < 0)
        {
            if (enumerator.ReadingCompleted)
            {
                if (enumerator.CurrentBits > 0)
                {
                    enumerator.Buffer <<= bitsPerDigit - enumerator.CurrentBits;
                    enumerator.Current = (int)enumerator.Buffer & enumerator.Mask;
                    enumerator.CurrentBits -= bitsPerDigit;
                    // Negative value indicates the extra bits in output sequence
                    return true;
                }
                return false;
            }
            int bytesToRead = ~offset / 8 + 1;
            int bitsToRead = bytesToRead * 8;
            uint dataRead = enumerator.ReadAtLeast(ref bytesToRead) >> (32 - bitsToRead);
            if (enumerator.CurrentBits == 0 && bytesToRead == 0)
                return false;
            enumerator.Buffer = (enumerator.Buffer << bitsToRead) | dataRead;
            enumerator.CurrentBits += bitsToRead;
            offset = enumerator.CurrentBits - bitsPerDigit;
            int bitsReadDiff = bitsToRead - bytesToRead * 8;
            if (bitsReadDiff > 0)
                enumerator.CurrentBits -= bitsReadDiff;
        }
        enumerator.Current = (int)(enumerator.Buffer >> offset) & enumerator.Mask;
        enumerator.CurrentBits -= bitsPerDigit;
        return true;
    }
    public static async ValueTask<bool> MoveNextAsync(IBase2NAsyncEnumerator enumerator, CancellationToken cancellationToken)
    {
        int bitsPerDigit = BitOperations.PopCount((uint)enumerator.Mask);
        int offset = enumerator.CurrentBits - bitsPerDigit;
        if (offset < 0)
        {
            if (enumerator.ReadingCompleted)
            {
                if (enumerator.CurrentBits > 0)
                {
                    enumerator.Buffer <<= bitsPerDigit - enumerator.CurrentBits;
                    enumerator.Current = (int)enumerator.Buffer & enumerator.Mask;
                    enumerator.CurrentBits -= bitsPerDigit;
                    // Negative value indicates the extra bits in output sequence
                    return true;
                }
                return false;
            }
            int bytesToRead = ~offset / 8 + 1;
            int bitsToRead = bytesToRead * 8;
            (uint data, int read) = await enumerator.ReadAtLeastAsync(bytesToRead, cancellationToken).ConfigureAwait(false);
            if (enumerator.CurrentBits == 0 && read == 0)
                return false;
            uint dataRead = data >> (32 - bitsToRead);
            enumerator.Buffer = (enumerator.Buffer << bitsToRead) | dataRead;
            enumerator.CurrentBits += bitsToRead;
            offset = enumerator.CurrentBits - bitsPerDigit;
            int bitsReadDiff = bitsToRead - read * 8;
            if (bitsReadDiff > 0)
                enumerator.CurrentBits -= bitsReadDiff;
        }
        enumerator.Current = (int)(enumerator.Buffer >> offset) & enumerator.Mask;
        enumerator.CurrentBits -= bitsPerDigit;
        return true;
    }
    internal static long TryGetPosition(this Stream stream)
    {
        try
        {
            return stream.Position;
        }
        catch
        {
            return -1;
        }
    }
}