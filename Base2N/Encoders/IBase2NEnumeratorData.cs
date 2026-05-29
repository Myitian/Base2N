namespace Base2N.Encoders;

public interface IBase2NEnumeratorData
{
    ulong Buffer { get; set; }
    int Mask { get; }
    int CurrentValue { get; set; }
    int CurrentBits { get; set; }
    bool ReadingCompleted { get; }
}