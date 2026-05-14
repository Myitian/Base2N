namespace Base2N.Encoders;

public interface IBase2NEnumerator : IBase2NEnumeratorData
{
    uint ReadData(ref int bytesToRead);
}