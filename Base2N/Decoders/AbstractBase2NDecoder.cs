using System.Numerics;

namespace Base2N.Decoders;

public abstract class AbstractBase2NDecoder : IBase2NDecoder
{
    protected ulong Buffer { get; set; }
    protected int CurrentBits { get; set; }
    public int Radix { get; }

    protected AbstractBase2NDecoder(int radix)
    {
        Base2NUtils.ValidateRadix(radix);
        Radix = radix;
    }

    public void WriteDigit(int digit, int extraBits = -1)
    {
        ObjectDisposedException.ThrowIf(CurrentBits < 0, this);
        extraBits = extraBits is int.MaxValue or < 0 ? 0 : extraBits;
        int mask = Radix - 1;
        int bitsPerDigit = BitOperations.PopCount((uint)mask);
        if (CurrentBits > 64 - bitsPerDigit)
            Flush();
        CurrentBits += bitsPerDigit - extraBits;
        Buffer = ((Buffer << bitsPerDigit) | (uint)(digit & mask)) >> extraBits;
    }
    public abstract void Flush();
    protected abstract void Dispose(bool disposing);
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    ~AbstractBase2NDecoder()
    {
        Dispose(false);
    }

    public static async ValueTask WriteDigitAsync<T>(T @this,
        int digit, int extraBits = -1, CancellationToken cancellationToken = default)
        where T : AbstractBase2NDecoder, IAsyncBase2NDecoder
    {
        ArgumentNullException.ThrowIfNull(@this);
        ObjectDisposedException.ThrowIf(@this.CurrentBits < 0, @this);
        extraBits = extraBits is int.MaxValue or < 0 ? 0 : extraBits;
        int mask = @this.Radix - 1;
        int bitsPerDigit = BitOperations.PopCount((uint)mask);
        if (@this.CurrentBits > 64 - bitsPerDigit)
            await @this.FlushAsync(cancellationToken).ConfigureAwait(false);
        @this.CurrentBits += bitsPerDigit - extraBits;
        @this.Buffer = ((@this.Buffer << bitsPerDigit) | (uint)(digit & mask)) >> extraBits;
    }
}