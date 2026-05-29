using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Base2N.Decoders;

public class BufferWriterBase2NDecoder(IBufferWriter<byte> writer, int radix) : AbstractBase2NDecoder(radix)
{
    public IBufferWriter<byte> Writer { get; } = writer;

    public override void Flush()
    {
        ObjectDisposedException.ThrowIf(CurrentBits < 0, this);
        (int byteCount, int remainingBits) = Math.DivRem(CurrentBits, 8);
        ulong output = Buffer << (64 - CurrentBits);
        if (BitConverter.IsLittleEndian)
            output = BinaryPrimitives.ReverseEndianness(output);
        Writer.Write(MemoryMarshal.AsBytes(new ReadOnlySpan<ulong>(ref output))[..byteCount]);
        CurrentBits = remainingBits;
    }
    protected override void Dispose(bool disposing)
    {
        if (CurrentBits >= 0)
        {
            Flush();
            CurrentBits = -1;
        }
    }
}