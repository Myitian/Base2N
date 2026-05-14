namespace Base2N;

public interface IBase2NAsyncEnumerator
{
    ulong Buffer { get; set; }
    int Mask { get; }
    int Current { get; set; }
    int CurrentBits { get; set; }
    bool ReadingCompleted { get; }
    ValueTask<(uint Data, int Read)> ReadDataAsync(int bytesToRead, CancellationToken cancellationToken);
}