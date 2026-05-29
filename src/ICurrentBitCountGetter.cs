namespace Base2N;

public interface ICurrentBitCountGetter
{
    /// <summary>
    /// The current number of bits in the buffer. Accuracy is not guaranteed during reading, but
    /// it must be accurate when the end of the buffer is reached. After reading, this value must
    /// be zero or negative, and its absolute value must equal the number of extra bits the last
    /// digit. If the absolute value is greater than the bit width, the decoder behavior is undefined.
    /// </summary>
    int CurrentBitCount { get; }
}