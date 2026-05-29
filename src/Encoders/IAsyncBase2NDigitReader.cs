namespace Base2N.Encoders;

/// <summary>
/// Pull-based async base-2^N digit reader.
/// </summary>
public interface IAsyncBase2NDigitReader : ICurrentBitCountGetter, IAsyncEnumerator<int>;