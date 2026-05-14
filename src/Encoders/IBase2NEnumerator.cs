namespace Base2N.Encoders;

public interface IBase2NEnumerator : IBase2NEnumeratorData, IEnumerator<int>
{
    uint ReadData(ref int bytesToRead);
}