namespace Base2N.Text.DigitCollections;

#pragma warning disable CA1711,CA1815
public readonly struct CharDigitCollection(ReadOnlyMemory<char> chars) : IDigitCollection<char>
{
    public ReadOnlyMemory<char> Chars { get; } = chars;

    public CharDigitCollection(string chars) : this(chars.AsMemory()) { }
    public CharDigitCollection(char[] chars) : this(chars.AsMemory()) { }

    public char GetDigit(int digit)
        => Chars.Span[digit];
    public ReadOnlyMemory<char> GetDigitString(int digit)
        => Chars.Slice(digit, 1);
}