using Base2N.Decoders;
using System.Numerics;

namespace Base2N.Streams;

public class Base2NEncoderStream<TBase2NDigitWriter> : WriteOnlyStream, IRadixGetter
    where TBase2NDigitWriter : IBase2NDigitWriter
{
    protected ulong BitBuffer { get; set; }
    protected int CurrentBits { get; set; }
    protected bool LeaveOpen { get; }
    protected bool Disposed { get; set; }
    public int Radix { get; }
    public TBase2NDigitWriter BaseWriter { get; }

    public Base2NEncoderStream(TBase2NDigitWriter writer, int radix, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull<TBase2NDigitWriter>(writer);
        Base2NUtils.ValidateRadix(radix);
        LeaveOpen = leaveOpen;
        Radix = radix;
        BaseWriter = writer;
    }

    public override void Write(byte[] buffer, int offset, int count)
        => Write(buffer.AsSpan(offset, count));
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        int mask = Radix - 1;
        int bitPerDigit = BitOperations.PopCount((uint)mask);
        ulong bitBuffer = BitBuffer;
        int currentBits = CurrentBits;
        foreach (byte value in buffer)
        {
            bitBuffer = (bitBuffer << 8) | value;
            currentBits += 8;
            while ((currentBits -= bitPerDigit) > 0)
                BaseWriter.WriteDigit((int)(bitBuffer >> currentBits) & mask);
            currentBits += bitPerDigit;
        }
        BitBuffer = bitBuffer;
        CurrentBits = currentBits;
    }
    public override void WriteByte(byte value)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        int mask = Radix - 1;
        int bitPerDigit = BitOperations.PopCount((uint)mask);
        ulong bitBuffer = (BitBuffer << 8) | value;
        int currentBits = CurrentBits + 8;
        while ((currentBits -= bitPerDigit) > 0)
            BaseWriter.WriteDigit((int)(bitBuffer >> currentBits) & mask);
        currentBits += bitPerDigit;
        BitBuffer = bitBuffer;
        CurrentBits = currentBits;
    }
    public override void Flush() { }
#pragma warning disable CA2215
    protected override void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            Disposed = true;
            Complete();
            if (!LeaveOpen)
                BaseWriter.Dispose();
        }
    }
    public void Complete()
    {
        int mask = Radix - 1;
        int bitPerDigit = BitOperations.PopCount((uint)mask);
        ulong buffer = BitBuffer;
        int currentBits = CurrentBits; ;
        if (currentBits == 0)
        {
            BaseWriter.WriteDigit(0, int.MaxValue);
            return;
        }
        while ((currentBits -= bitPerDigit) > 0)
            BaseWriter.WriteDigit((int)(buffer >> currentBits) & mask);
        int extraBits = -currentBits;
        BaseWriter.WriteDigit((int)(buffer << extraBits) & mask, extraBits);
        CurrentBits = 0;
    }
}
#pragma warning disable CA1711
public static class Base2NEncoderStream
{
    public static AsyncBase2NEncoderStream<TBase2NDigitWriter> CreateAsync<TBase2NDigitWriter>(TBase2NDigitWriter writer, int radix, bool leaveOpen = false)
        where TBase2NDigitWriter : IBase2NDigitWriter, IAsyncBase2NDigitWriter
    {
        return new(writer, radix, leaveOpen);
    }
    public static Base2NEncoderStream<TBase2NDigitWriter> Create<TBase2NDigitWriter>(TBase2NDigitWriter writer, int radix, bool leaveOpen = false)
        where TBase2NDigitWriter : IBase2NDigitWriter
    {
        return new(writer, radix, leaveOpen);
    }
}