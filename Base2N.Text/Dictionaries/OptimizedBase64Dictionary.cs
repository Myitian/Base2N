using System.Collections;

namespace Base2N.Text.Dictionaries;

#pragma warning disable CA1043,CA1815
public readonly struct OptimizedBase64Dictionary(char char62, char char63) : IReadOnlyDictionary<char, int>
{
    public const string OptimizedPrefix = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public int this[char key]
        => TryGetValue(key, out int value) ? value : throw new KeyNotFoundException();
    public IEnumerable<char> Keys => $"{OptimizedPrefix}{char62}{char63}";
    public IEnumerable<int> Values => Enumerable.Range(0, 64);
    public int Count => 64;

    public bool ContainsKey(char key)
    {
        return key == char62
            || key == char63
            || key is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9');
    }
    public bool TryGetValue(char key, out int value)
    {
        value = -1;
        if (key == char62)
            value = 62;
        else if (key == char63)
            value = 63;
        else if (key >= '0')
        {
            if (key <= '9')
                value = key - '0' + 52;
            else if (key >= 'A')
            {
                if (key <= 'Z')
                    value = key - 'A';
                else if (key is >= 'a' and <= 'z')
                    value = key - 'a' + 26;
            }
        }
        return value >= 0;
    }
    public IEnumerator<KeyValuePair<char, int>> GetEnumerator()
    {
        for (int i = 0; i < 62; i++)
            yield return new(OptimizedPrefix[i], i);
        yield return new(char62, 62);
        yield return new(char63, 63);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static OptimizedBase64Dictionary Create(ReadOnlySpan<char> charSet)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(charSet.Length, 64);
        if (!charSet.StartsWith(OptimizedPrefix))
            throw new ArgumentException("Unable to create optimized mapping", nameof(charSet));
        return new(charSet[62], charSet[63]);
    }
}