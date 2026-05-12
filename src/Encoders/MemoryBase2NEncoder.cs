using System.Buffers.Binary;
using System.Collections;
using System.Numerics;

namespace Base2N.Encoders;

public readonly struct MemoryBase2NEncoder : IReadOnlyList<int>
{
    public ReadOnlyMemory<byte> Data { get; }
    public long LongCount { get; }
    public int Count => checked((int)LongCount);
    public int Radix { get; }
    public int ExtraBits { get; }

    public int this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
            return Base2NUtils.DigitAt(Data.Span, Radix, index);
        }
    }

    public MemoryBase2NEncoder(ReadOnlyMemory<byte> data, int radix)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(radix, 2);
        if (BitOperations.PopCount((uint)radix) != 1)
            throw new ArgumentException("Radix must be a power of two.", nameof(radix));
        Data = data;
        Radix = radix;
        int bitPerDigit = BitOperations.TrailingZeroCount((uint)radix);
        (LongCount, long extraBitsU64) = Math.DivRem((long)data.Length * 8, bitPerDigit);
        int extraBits = (int)extraBitsU64;
        if (extraBits != 0)
        {
            LongCount++;
            ExtraBits = bitPerDigit - extraBits;
        }
    }

    public Enumerator GetEnumerator()
        => new(this);
    IEnumerator<int> IEnumerable<int>.GetEnumerator()
        => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator(MemoryBase2NEncoder encoder)
        : IEnumerator<int>, IBase2NEnumerator
    {
        private readonly ReadOnlyMemory<byte> _data = encoder.Data;
        private ulong _buffer;
        private readonly int _mask = encoder.Radix - 1;
        private int _currentBits = 0;
        private int _index = 0;

        public readonly int CurrentBits => _currentBits;
        public int Current { readonly get; private set; }
        readonly object IEnumerator.Current => Current;

        ulong IBase2NEnumerator.Buffer { readonly get => _buffer; set => _buffer = value; }
        readonly int IBase2NEnumerator.Mask => _mask;
        int IBase2NEnumerator.Current { readonly get => Current; set => Current = value; }
        int IBase2NEnumerator.CurrentBits { readonly get => _currentBits; set => _currentBits = value; }
        public readonly bool ReadingCompleted => _index < 0;

        public bool MoveNext()
        {
            return Base2NUtils.MoveNext(ref this);
        }
        public void Reset()
        {
            _currentBits = 0;
            _index = 0;
        }
        public readonly void Dispose() { }

        uint IBase2NEnumerator.ReadAtLeast(ref int bytesToRead)
        {
            int remaining = _data.Length - _index;
            Span<byte> buffer = stackalloc byte[4];
            buffer.Clear();
            if (remaining <= bytesToRead)
            {
                _data.Span[_index..].CopyTo(buffer);
                _index = -1;
                bytesToRead = remaining;
            }
            else
            {
                _data.Span.Slice(_index, bytesToRead).CopyTo(buffer);
                _index += bytesToRead;
            }
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
    }
}