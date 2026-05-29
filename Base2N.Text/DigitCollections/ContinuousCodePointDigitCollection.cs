using System.Numerics;

namespace Base2N.Text.DigitCollections;

#pragma warning disable CA1711
public readonly struct ContinuousCodePointDigitCollection(int firstCodePoint)
    : IDigitCollection<int>, IEquatable<ContinuousCodePointDigitCollection>, IEqualityOperators<ContinuousCodePointDigitCollection, ContinuousCodePointDigitCollection, bool>
{
    public int FirstCodePoint { get; } = firstCodePoint;
    public int GetDigit(int digit) => FirstCodePoint + digit;

    public bool Equals(ContinuousCodePointDigitCollection other)
        => FirstCodePoint == other.FirstCodePoint;
    public override bool Equals(object? obj)
        => obj is ContinuousCodePointDigitCollection other && Equals(other);
    public override int GetHashCode()
        => FirstCodePoint;
    public static bool operator ==(ContinuousCodePointDigitCollection left, ContinuousCodePointDigitCollection right)
        => left.Equals(right);
    public static bool operator !=(ContinuousCodePointDigitCollection left, ContinuousCodePointDigitCollection right)
        => !left.Equals(right);
}