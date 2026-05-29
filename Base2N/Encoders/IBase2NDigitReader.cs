namespace Base2N.Encoders;

/// <summary>
/// Pull-based base-2^N digit reader.
/// </summary>
public interface IBase2NDigitReader : ICurrentBitCountGetter, IEnumerator<int>;