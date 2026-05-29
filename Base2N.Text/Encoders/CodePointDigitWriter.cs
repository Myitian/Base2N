using Base2N.Text.DigitCollections;
using Base2N.Text.ExtraBitsHandlers;

namespace Base2N.Text.Encoders;

public sealed class CodePointDigitWriter<TDigitCollection, TExtraBitsHandler> : TextDigitWriter<TExtraBitsHandler>
    where TDigitCollection : IDigitCollection<int>
    where TExtraBitsHandler : IExtraBitsHandler
{
    public TDigitCollection DigitCollection { get; }

    public CodePointDigitWriter(
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
            WriteCodePoint(DigitCollection.GetDigit(digit));
        WriteExtraBits(extraBits);
    }
    private void WriteCodePoint(int codepoint)
    {
        if (codepoint is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(nameof(codepoint), "Must be a valid Unicode code point (0 to 0x10FFFF).");
        if (codepoint <= char.MaxValue)
            Writer.Write((char)codepoint);
        else
        {
            Writer.Write((char)((codepoint + ((0xD800u - 0x40u) << 10)) >> 10));
            Writer.Write((char)((codepoint & 0x3FFu) + 0xDC00u));
        }
    }
    public override async ValueTask WriteDigitAsync(int digit, int extraBits = -1, CancellationToken cancellationToken = default)
    {
        if (extraBits == int.MaxValue)
            extraBits = 0;
        else
            await WriteCodePointAsync(DigitCollection.GetDigit(digit)).ConfigureAwait(false);
        await WriteExtraBitsAsync(extraBits, cancellationToken).ConfigureAwait(false);
    }
    private async ValueTask WriteCodePointAsync(int codepoint)
    {
        if (codepoint is < 0 or > 0x10FFFF)
            throw new ArgumentOutOfRangeException(nameof(codepoint), "Must be a valid Unicode code point (0 to 0x10FFFF).");
        if (codepoint <= char.MaxValue)
            await Writer.WriteAsync((char)codepoint).ConfigureAwait(false);
        else
        {
            await Writer.WriteAsync((char)((codepoint + ((0xD800u - 0x40u) << 10)) >> 10)).ConfigureAwait(false);
            await Writer.WriteAsync((char)((codepoint & 0x3FFu) + 0xDC00u)).ConfigureAwait(false);
        }
    }
}