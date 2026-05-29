namespace Base2N.Text.DigitCollections;

#pragma warning disable CA1711
public interface IDigitCollection<T>
    where T : allows ref struct
{
    public T GetDigit(int digit);
}