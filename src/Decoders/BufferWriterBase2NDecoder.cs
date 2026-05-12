using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Base2N.Decoders;

public class BufferWriterBase2NDecoder : IBase2NDecoder
{
    private ulong _buffer;
    private int _currentBits;

    public IBufferWriter<byte> Writer { get; }
    public int Radix { get; }

    public BufferWriterBase2NDecoder(IBufferWriter<byte> writer, int radix)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(radix, 2);
        if (BitOperations.PopCount((uint)radix) != 1)
            throw new ArgumentException("Radix must be a power of two.", nameof(radix));
        Writer = writer;
        Radix = radix;
    }

    public void WriteDigit(int digit, int extraBits = 0)
    {
        ObjectDisposedException.ThrowIf(_currentBits < 0, typeof(BufferWriterBase2NDecoder));
        int mask = Radix - 1;
        int bitsPerDigit = BitOperations.PopCount((uint)mask);
        if (_currentBits > 64 - bitsPerDigit)
            Flush();
        _currentBits += bitsPerDigit - extraBits;
        _buffer = ((_buffer << bitsPerDigit) | (uint)(digit & mask)) >> extraBits;
    }
    public void Flush()
    {
        ObjectDisposedException.ThrowIf(_currentBits < 0, typeof(BufferWriterBase2NDecoder));
        (int byteCount, int remainingBits) = Math.DivRem(_currentBits, 8);
        ulong output = _buffer << (64 - _currentBits);
        if (BitConverter.IsLittleEndian)
            output = BinaryPrimitives.ReverseEndianness(output);
        Writer.Write(MemoryMarshal.AsBytes(new ReadOnlySpan<ulong>(ref output))[..byteCount]);
        _currentBits = remainingBits;
    }
    public virtual void Dispose()
    {
        if (_currentBits >= 0)
        {
            Flush();
            _currentBits = -1;
        }
        GC.SuppressFinalize(this);
    }
}