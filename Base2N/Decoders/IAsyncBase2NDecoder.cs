namespace Base2N.Decoders;

public interface IAsyncBase2NDecoder : IRadixGetter, IAsyncBase2NDigitWriter
{
    ValueTask FlushAsync(CancellationToken cancellationToken = default);
}