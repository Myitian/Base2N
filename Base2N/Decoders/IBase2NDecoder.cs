namespace Base2N.Decoders;

public interface IBase2NDecoder : IRadixGetter, IBase2NDigitWriter
{
    void Flush();
}