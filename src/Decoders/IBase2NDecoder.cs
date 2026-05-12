namespace Base2N.Decoders;

public interface IBase2NDecoder : IDisposable
{
    int Radix { get; }
    void WriteDigit(int digit, int extraBits = 0);
    void Flush();
}