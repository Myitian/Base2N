namespace Base2N.Text.DigitCollections;

#pragma warning disable CA1711,CA1815
public readonly struct StringDigitCollection(ReadOnlyMemory<string> strings) : IDigitCollection<string>
{
    public ReadOnlyMemory<string> Strings { get; } = strings;

    public StringDigitCollection(string[] chars) : this(chars.AsMemory()) { }

    public string GetDigit(int digit)
        => Strings.Span[digit];
    public ReadOnlyMemory<char> GetDigitString(int digit)
        => Strings.Span[digit].AsMemory();
}