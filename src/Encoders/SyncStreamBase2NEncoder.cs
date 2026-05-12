using System.Buffers.Binary;
using System.Collections;

namespace Base2N.Encoders;

public readonly struct SyncStreamBase2NEncoder(Stream stream, int radix, bool leaveOpen = false) : IEnumerable<int>
{
    public Stream Stream { get; } = stream;
    public int Radix { get; } = radix;
    private readonly bool _leaveOpen = leaveOpen;

    public Enumerator GetEnumerator()
        => new(this);
    IEnumerator<int> IEnumerable<int>.GetEnumerator()
        => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public struct Enumerator(SyncStreamBase2NEncoder encoder)
        : IEnumerator<int>, IBase2NEnumerator
    {
        private Stream? _stream = encoder.Stream;
        private readonly long _position = encoder.Stream.TryGetPosition();
        private ulong _buffer;
        private readonly int _mask = encoder.Radix - 1;
        private int _currentBits = 0;
        private readonly bool _leaveOpen = encoder._leaveOpen;

        public readonly int CurrentBits => _currentBits;
        public int Current { readonly get; private set; }
        readonly object IEnumerator.Current => Current;

        ulong IBase2NEnumerator.Buffer { readonly get => _buffer; set => _buffer = value; }
        readonly int IBase2NEnumerator.Mask => _mask;
        int IBase2NEnumerator.Current { readonly get => Current; set => Current = value; }
        int IBase2NEnumerator.CurrentBits { readonly get => _currentBits; set => _currentBits = value; }
        public bool ReadingCompleted { readonly get; private set; } = false;

        public bool MoveNext()
        {
            ObjectDisposedException.ThrowIf(_stream is null, typeof(Enumerator));
            return Base2NUtils.MoveNext(ref this);
        }
        public void Reset()
        {
            ObjectDisposedException.ThrowIf(_stream is null, typeof(Enumerator));
            if (_position < 0)
                throw new NotSupportedException();
            _stream.Position = _position;
            _currentBits = 0;
            ReadingCompleted = false;
        }
        public void Dispose()
        {
            if (!_leaveOpen)
                _stream?.Dispose();
            _stream = null;
            GC.SuppressFinalize(this);
        }
        public uint ReadAtLeast(ref int bytesToRead)
        {
            ObjectDisposedException.ThrowIf(_stream is null, typeof(Enumerator));

            int original = bytesToRead;
            Span<byte> buffer = stackalloc byte[4];
            buffer.Clear();
            bytesToRead = _stream.ReadAtLeast(buffer[..bytesToRead], bytesToRead, false);
            if (original != bytesToRead)
                ReadingCompleted = true;
            return BinaryPrimitives.ReadUInt32BigEndian(buffer);
        }
    }
}