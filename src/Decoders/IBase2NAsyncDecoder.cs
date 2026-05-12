namespace Base2N.Decoders;

public interface IBase2NAsyncDecoder : IAsyncDisposable
{
    int Radix { get; }
    ValueTask WriteDigitAsync(int digit, int extraBits = 0, CancellationToken cancellationToken = default);
    ValueTask FlushAsync(CancellationToken cancellationToken = default);
}