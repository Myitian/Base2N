namespace Base2N.Encoders;

public interface IBase2NRandomAccessibleEncoder : IRadixGetter, IReadOnlyList<int>
{
    long LongCount { get; }
    int ExtraBits { get; }
}