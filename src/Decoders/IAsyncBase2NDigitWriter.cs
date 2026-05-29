namespace Base2N.Decoders;

public interface IAsyncBase2NDigitWriter : IAsyncDisposable
{
    /// <param name="digit">The digit to write.</param>
    /// <param name="extraBits">
    /// A negative value indicates that the end has not been reached; zero or a positive value
    /// indicates that the end has been reached; <see cref="int.MaxValue"/> indicates that the
    /// end has been reached and the <paramref name="digit"/> parameter is ignored, which is
    /// equivalent to passing 0 in <paramref name="extraBits"/> in the previous call.
    /// </param>
    ValueTask WriteDigitAsync(int digit, int extraBits = -1, CancellationToken cancellationToken = default);
}