using System.Buffers.Binary;
using System.Collections;

namespace Base2N.Encoders;

public sealed class StreamBase2NEncoder : IEnumerable<int>, IAsyncEnumerable<int>
{
    public Stream Stream { get; }
    public int Radix { get; }
    private readonly bool _leaveOpen;

    public StreamBase2NEncoder(Stream stream, int radix, bool leaveOpen = false)
    {
        Base2NUtils.ValidateRadix(radix);
        Stream = stream;
        Radix = radix;
        _leaveOpen = leaveOpen;
    }

    public Enumerator GetEnumerator()
        => new(this, default);
    IEnumerator<int> IEnumerable<int>.GetEnumerator()
        => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
    public Enumerator GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new(this, cancellationToken);
    IAsyncEnumerator<int> IAsyncEnumerable<int>.GetAsyncEnumerator(CancellationToken cancellationToken)
        => GetAsyncEnumerator(cancellationToken);

    public sealed class Enumerator(StreamBase2NEncoder encoder, CancellationToken cancellationToken, bool noReset = false)
        : IEnumerator<int>, IBase2NEnumerator, IAsyncEnumerator<int>, IBase2NAsyncEnumerator
    {
        private readonly CancellationToken _cancellationToken = cancellationToken;
        private byte[]? _asyncBuffer;
        private Stream? _stream = encoder.Stream;
        private readonly long _position = noReset ? -1 : encoder.Stream.TryGetPosition();
        private ulong _buffer;
        private readonly int _mask = encoder.Radix - 1;
        private int _currentBits = 0;
        private readonly bool _leaveOpen = encoder._leaveOpen;

        public int CurrentBits => _currentBits;
        public int Current { get; private set; }
        object IEnumerator.Current => Current;

        ulong IBase2NEnumerator.Buffer { get => _buffer; set => _buffer = value; }
        int IBase2NEnumerator.Mask => _mask;
        int IBase2NEnumerator.Current { get => Current; set => Current = value; }
        int IBase2NEnumerator.CurrentBits { get => _currentBits; set => _currentBits = value; }
        ulong IBase2NAsyncEnumerator.Buffer { get => _buffer; set => _buffer = value; }
        int IBase2NAsyncEnumerator.Mask => _mask;
        int IBase2NAsyncEnumerator.Current { get => Current; set => Current = value; }
        int IBase2NAsyncEnumerator.CurrentBits { get => _currentBits; set => _currentBits = value; }
        public bool ReadingCompleted { get; private set; } = false;

        public bool MoveNext()
        {
            ObjectDisposedException.ThrowIf(_stream is null, typeof(Enumerator));
            return Base2NUtils.MoveNext(this);
        }
        public async ValueTask<bool> MoveNextAsync()
        {
            ObjectDisposedException.ThrowIf(_stream is null, typeof(Enumerator));
            return await Base2NUtils.MoveNextAsync(this, _cancellationToken).ConfigureAwait(false);
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
        }
        public async ValueTask DisposeAsync()
        {
            if (!_leaveOpen && _stream is not null)
                await _stream.DisposeAsync().ConfigureAwait(false);
            _stream = null;
        }
        public uint ReadData(ref int bytesToRead)
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
        public async ValueTask<(uint, int)> ReadDataAsync(int bytesToRead, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_stream is null, typeof(Enumerator));

            int original = bytesToRead;
            Memory<byte> buffer = _asyncBuffer ??= new byte[4];
            buffer.Span.Clear();
            bytesToRead = await _stream.ReadAtLeastAsync(buffer[..bytesToRead], bytesToRead, false, cancellationToken).ConfigureAwait(false);
            if (original != bytesToRead)
                ReadingCompleted = true;
            return (BinaryPrimitives.ReadUInt32BigEndian(buffer.Span), bytesToRead);
        }
    }
}