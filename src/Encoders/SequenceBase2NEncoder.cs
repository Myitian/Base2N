using System.Buffers;
using System.Buffers.Binary;
using System.Collections;

namespace Base2N.Encoders;

public readonly struct SequenceBase2NEncoder : IReadOnlyList<int>
{
    public ReadOnlySequence<byte> Data { get; }
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
            return Base2NUtils.DigitAt(Data, Radix, index);
        }
    }

    public SequenceBase2NEncoder(ReadOnlySequence<byte> data, int radix)
    {
        Base2NUtils.ValidateRadix(radix);
        Data = data;
        Radix = radix;
        (LongCount, ExtraBits) = Base2NUtils.GetCountAndExtraBits(data.Length, radix);
    }

    public Enumerator GetEnumerator()
        => new(this);
    IEnumerator<int> IEnumerable<int>.GetEnumerator()
        => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator(SequenceBase2NEncoder encoder)
        : IEnumerator<int>, IBase2NEnumerator
    {
        private readonly ReadOnlySequence<byte> _original = encoder.Data;
        private ReadOnlySequence<byte> _data = encoder.Data;
        private ulong _buffer;
        private readonly int _mask = encoder.Radix - 1;
        private int _currentBits = 0;

        public readonly int CurrentBits => _currentBits;
        public int Current { readonly get; private set; }
        readonly object IEnumerator.Current => Current;

        ulong IBase2NEnumeratorData.Buffer { readonly get => _buffer; set => _buffer = value; }
        readonly int IBase2NEnumeratorData.Mask => _mask;
        int IBase2NEnumeratorData.Current { readonly get => Current; set => Current = value; }
        int IBase2NEnumeratorData.CurrentBits { readonly get => _currentBits; set => _currentBits = value; }
        public bool ReadingCompleted { get; private set; }

        public bool MoveNext()
            => Base2NUtils.MoveNext(ref this);
        public void Reset()
        {
            _currentBits = 0;
            _data = _original;
            ReadingCompleted = false;
        }
        public readonly void Dispose() { }

        public uint ReadData(ref int bytesToRead)
        {
            long remaining = _data.Length;
            Span<byte> buffer = stackalloc byte[4];
            buffer.Clear();
            if (remaining <= bytesToRead)
            {
                _data.CopyTo(buffer);
                ReadingCompleted = true;
                bytesToRead = (int)remaining;
            }
            else
            {
                SequencePosition pos = _data.GetPosition(bytesToRead);
                _data.Slice(_data.Start, pos).CopyTo(buffer);
                _data = _data.Slice(pos);
            }
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
    }
}