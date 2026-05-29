using Base2N.Text.DigitCollections;
using Base2N.Text.ExtraBitsHandlers;

namespace Base2N.Text.Encoders;

public sealed class CharDigitWriter<TDigitCollection, TExtraBitsHandler> : TextDigitWriter<TExtraBitsHandler>
    where TDigitCollection : IDigitCollection<char>
    where TExtraBitsHandler : IExtraBitsHandler
{
    public TDigitCollection DigitCollection { get; }

    public CharDigitWriter(
        TextWriter writer,
        TDigitCollection digitCollection,
        TExtraBitsHandler extraBitsHandler,
        bool leaveOpen = false) : base(writer, extraBitsHandler, leaveOpen)
    {
        ArgumentNullException.ThrowIfNull<TDigitCollection>(digitCollection);
        DigitCollection = digitCollection;
    }

    public override void WriteDigit(int digit, int extraBits = -1)
    {
        if (extraBits == int.MaxValue)
            extraBits = 0;
        else
            Writer.Write(DigitCollection.GetDigit(digit));
        WriteExtraBits(extraBits);
    }
    public override async ValueTask WriteDigitAsync(int digit, int extraBits = -1, CancellationToken cancellationToken = default)
    {
        if (extraBits == int.MaxValue)
            extraBits = 0;
        else
            await Writer.WriteAsync(DigitCollection.GetDigit(digit)).ConfigureAwait(false);
        await WriteExtraBitsAsync(extraBits, cancellationToken).ConfigureAwait(false);
    }
}