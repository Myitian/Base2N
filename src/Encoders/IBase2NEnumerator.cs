namespace Base2N;

public interface IBase2NEnumerator
{
    ulong Buffer { get; set; }
    int Mask { get; }
    int Current { get; set; }
    int CurrentBits { get; set; }
    bool ReadingCompleted { get; }
    uint ReadData(ref int bytesToRead);
}