namespace Base2N.Encoders;

public interface IBase2NRandomAccessibleEncoder : IBase2NEncoder, IReadOnlyList<int>
{
    long LongCount { get; }
    int ExtraBits { get; }
}