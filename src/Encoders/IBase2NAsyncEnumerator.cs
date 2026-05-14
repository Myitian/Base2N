namespace Base2N.Encoders;

public interface IBase2NAsyncEnumerator : IBase2NEnumeratorData, IAsyncEnumerator<int>
{
    ValueTask<(uint Data, int Read)> ReadDataAsync(int bytesToRead, CancellationToken cancellationToken);
}