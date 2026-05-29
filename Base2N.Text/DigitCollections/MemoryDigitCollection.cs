namespace Base2N.Text.DigitCollections;

#pragma warning disable CA1711,CA1815
public readonly struct MemoryDigitCollection(ReadOnlyMemory<ReadOnlyMemory<char>> strings) : IDigitCollection<ReadOnlyMemory<char>>
{
    public ReadOnlyMemory<ReadOnlyMemory<char>> Strings { get; } = strings;

    public MemoryDigitCollection(ReadOnlyMemory<char>[] chars) : this(chars.AsMemory()) { }

    public ReadOnlyMemory<char> GetDigit(int digit)
        => Strings.Span[digit];
    public ReadOnlyMemory<char> GetDigitString(int digit)
        => Strings.Span[digit];
}