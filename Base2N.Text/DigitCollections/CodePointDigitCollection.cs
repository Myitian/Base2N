namespace Base2N.Text.DigitCollections;

#pragma warning disable CA1711,CA1815
public readonly struct CodePointDigitCollection(ReadOnlyMemory<int> codePoints) : IDigitCollection<int>
{
    public ReadOnlyMemory<int> CodePoints { get; } = codePoints;

    public CodePointDigitCollection(int[] codePoints) : this(codePoints.AsMemory()) { }

    public int GetDigit(int digit)
        => CodePoints.Span[digit];
}