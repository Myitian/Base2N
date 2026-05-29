using Base2N.Text.DigitCollections;
using System.Collections;
using System.Numerics;

namespace Base2N.Text.Dictionaries;

public readonly struct ContinuousCodePointDictionary(int firstCodePoint, int count)
    : IDigitCollection<int>, IReadOnlyDictionary<int, int>, IEquatable<ContinuousCodePointDictionary>, IEqualityOperators<ContinuousCodePointDictionary, ContinuousCodePointDictionary, bool>
{
    public int this[int key]
        => TryGetValue(key, out int value) ? value : throw new KeyNotFoundException();
    public IEnumerable<int> Keys => Enumerable.Range(FirstCodePoint, Count);
    public IEnumerable<int> Values => Enumerable.Range(0, Count);
    public int FirstCodePoint { get; } = firstCodePoint;
    public int Count { get; } = count;

    public int GetDigit(int digit)
        => FirstCodePoint + digit;
    public bool ContainsKey(int key)
        => key >= FirstCodePoint && key < FirstCodePoint + Count;
    public bool TryGetValue(int key, out int value)
    {
        value = key - FirstCodePoint;
        return value >= 0 && value < Count;
    }
    public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return new(i + FirstCodePoint, i);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(ContinuousCodePointDictionary other)
        => FirstCodePoint == other.FirstCodePoint && Count == other.Count;
    public override bool Equals(object? obj)
        => obj is ContinuousCodePointDictionary other && Equals(other);
    public override int GetHashCode()
        => HashCode.Combine(FirstCodePoint, Count);
    public static bool operator ==(ContinuousCodePointDictionary left, ContinuousCodePointDictionary right)
        => left.Equals(right);
    public static bool operator !=(ContinuousCodePointDictionary left, ContinuousCodePointDictionary right)
        => !left.Equals(right);
}