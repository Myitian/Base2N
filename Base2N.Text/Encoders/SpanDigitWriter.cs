using Base2N.Text.DigitCollections;
using Base2N.Text.ExtraBitsHandlers;
using System.Buffers;

namespace Base2N.Text.Encoders;

public sealed class SpanDigitWriter<TDigitCollection, TExtraBitsHandler> : TextDigitWriter<TExtraBitsHandler>
    where TDigitCollection : IDigitCollection<ReadOnlySpan<char>>
    where TExtraBitsHandler : IExtraBitsHandler
{
    public TDigitCollection DigitCollection { get; }

    public SpanDigitWriter(
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
        {
            ReadOnlySpan<char> digitSpan = DigitCollection.GetDigit(digit);
            char[] digitArray = ArrayPool<char>.Shared.Rent(digitSpan.Length);
            try
            {
                digitSpan.CopyTo(digitArray);
                await Writer.WriteAsync(digitArray.AsMemory(0, digitSpan.Length), cancellationToken)
                    .ConfigureAwait(false);

            }
            finally
            {
                ArrayPool<char>.Shared.Return(digitArray);
            }
        }
        await WriteExtraBitsAsync(extraBits, cancellationToken).ConfigureAwait(false);
    }
}