using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Numerics;

namespace Base2N.Encoders;

public class PipeBase2NEncoder : IAsyncEnumerable<int>
{
    public PipeReader Reader { get; }
    public int Radix { get; }

    public PipeBase2NEncoder(PipeReader reader, int radix)
    {
        Base2NUtils.ValidateRadix(radix);
        Reader = reader;
        Radix = radix;
    }
    public PipeBase2NEncoder(ReadOnlySequence<byte> data, int radix)
        : this(PipeReader.Create(data), radix) { }
    public PipeBase2NEncoder(Stream stream, int radix, StreamPipeReaderOptions? options = null)
        : this(PipeReader.Create(stream, options), radix) { }

    public Enumerator GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new(this, cancellationToken);
    IAsyncEnumerator<int> IAsyncEnumerable<int>.GetAsyncEnumerator(CancellationToken cancellationToken)
        => GetAsyncEnumerator(cancellationToken);

    public sealed class Enumerator(PipeBase2NEncoder encoder, CancellationToken cancellationToken)
        : IAsyncEnumerator<int>, IBase2NAsyncEnumerator, IDisposable
    {
        private readonly CancellationToken _cancellationToken = cancellationToken;
        private PipeReader? _reader = encoder.Reader;
        private ulong _buffer;
        private readonly int _mask = encoder.Radix - 1;
        private int _currentBits;

        public int CurrentBits => _currentBits;
        public int Current { get; private set; }

        ulong IBase2NAsyncEnumerator.Buffer { get => _buffer; set => _buffer = value; }
        int IBase2NAsyncEnumerator.Mask => _mask;
        int IBase2NAsyncEnumerator.Current { get => Current; set => Current = value; }
        int IBase2NAsyncEnumerator.CurrentBits { get => _currentBits; set => _currentBits = value; }
        public bool ReadingCompleted { get; private set; }

        public async ValueTask<bool> MoveNextAsync()
        {
            ObjectDisposedException.ThrowIf(_reader is null, typeof(Enumerator));
            return await Base2NUtils.MoveNextAsync(this, _cancellationToken).ConfigureAwait(false);
        }
        public void Reset()
            => throw new NotSupportedException();
        public void Dispose()
        {
            _reader?.Complete();
            _reader = null;
        }
        public async ValueTask DisposeAsync()
        {
            if (_reader is not null)
            {
                await _reader.CompleteAsync().ConfigureAwait(false);
                _reader = null;
            }
        }

        public async ValueTask<(uint Data, int Read)> ReadDataAsync(int bytesToRead, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_reader is null, typeof(Enumerator));
            ReadResult result = await _reader.ReadAtLeastAsync(bytesToRead, cancellationToken).ConfigureAwait(false);
            if (result.IsCompleted || result.IsCanceled)
            {
                ReadingCompleted = true;
                if (result.Buffer.IsEmpty)
                {
                    await _reader.CompleteAsync().ConfigureAwait(false);
                    return (0, 0);
                }
            }

            long read = Math.Min(bytesToRead, result.Buffer.Length);
            Span<byte> buffer = stackalloc byte[4];
            buffer.Clear();
            result.Buffer.Slice(0, read).CopyTo(buffer);
            uint data = BinaryPrimitives.ReadUInt32BigEndian(buffer);
            _reader.AdvanceTo(result.Buffer.GetPosition(read));
            return (data, (int)read);
        }
    }
}