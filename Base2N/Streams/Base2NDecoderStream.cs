using Base2N.Encoders;
using System.Numerics;

namespace Base2N.Streams;

public class Base2NDecoderStream<TBase2NDigitReader> : ReadOnlyStream, IRadixGetter
    where TBase2NDigitReader : IBase2NDigitReader
{
    protected ulong BitBuffer { get; set; }
    protected int CurrentBits { get; set; }
    protected bool LeaveOpen { get; }
    protected bool ReadCompleted { get; set; }
    protected bool Disposed { get; set; }
    public int Radix { get; }
    public TBase2NDigitReader BaseReader { get; }

    public Base2NDecoderStream(TBase2NDigitReader reader, int radix, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull<TBase2NDigitReader>(reader);
        Base2NUtils.ValidateRadix(radix);
        LeaveOpen = leaveOpen;
        Radix = radix;
        BaseReader = reader;
    }

    public override int Read(byte[] buffer, int offset, int count)
        => Read(buffer.AsSpan(offset, count));
    public override int Read(Span<byte> buffer)
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
            bool hasNext = BaseReader.MoveNext();
            if (!hasNext)
            {
                ReadCompleted = true;
                int offset = -BaseReader.CurrentBitCount;
                bitBuffer >>= offset;
                currentBits -= offset;
                break;
            }
            bitBuffer = (bitBuffer << bitPerDigit) | (uint)BaseReader.Current;
            currentBits += bitPerDigit;
        }
        int i = 0;
        while ((currentBits -= 8) >= 0 && i < buffer.Length)
            buffer[i++] = (byte)(bitBuffer >> currentBits);
        BitBuffer = bitBuffer;
        CurrentBits = currentBits + 8;
        return i;
    }
    public override int ReadByte()
    {
        byte result = default;
        return Read(new Span<byte>(ref result)) == 0 ? -1 : result;
    }
#pragma warning disable CA2215
    protected override void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            Disposed = true;
            if (!LeaveOpen)
                BaseReader.Dispose();
        }
    }
}
#pragma warning disable CA1711
public static class Base2NDecoderStream
{
    public static AsyncBase2NDecoderStream<TBase2NDigitReader> CreateAsync<TBase2NDigitReader>(TBase2NDigitReader reader, int radix, bool leaveOpen = false)
        where TBase2NDigitReader : IBase2NDigitReader, IAsyncBase2NDigitReader
    {
        return new(reader, radix, leaveOpen);
    }
    public static Base2NDecoderStream<TBase2NDigitReader> Create<TBase2NDigitReader>(TBase2NDigitReader reader, int radix, bool leaveOpen = false)
        where TBase2NDigitReader : IBase2NDigitReader
    {
        return new(reader, radix, leaveOpen);
    }
}