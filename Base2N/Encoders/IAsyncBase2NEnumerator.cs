namespace Base2N.Encoders;

public interface IAsyncBase2NEnumerator : IBase2NEnumeratorData, IAsyncBase2NDigitReader, IAsyncEnumerator<int>
{
    ValueTask<(uint Data, int Read)> ReadDataAsync(int bytesToRead, CancellationToken cancellationToken);
}