namespace Base2N.Encoders;

public interface IBase2NAsyncEnumerator : IBase2NEnumeratorData
{
    ValueTask<(uint Data, int Read)> ReadDataAsync(int bytesToRead, CancellationToken cancellationToken);
}