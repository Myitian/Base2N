using Base2N.Decoders;
using Base2N.Encoders;

namespace Base2N;

public static class Base2NProcessor
{
    public static void Process(
        IBase2NDigitReader digitReader,
        IBase2NDigitWriter digitWriter)
    {
        ArgumentNullException.ThrowIfNull(digitReader);
        ArgumentNullException.ThrowIfNull(digitWriter);
        if (digitReader.MoveNext())
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!digitReader.MoveNext())
                {
                    digitWriter.WriteDigit(last, -digitReader.CurrentBitCount);
                    break;
                }
                digitWriter.WriteDigit(last);
                last = digitReader.Current;
            }
        }
    }
    public static void Process<TWriter>(
        IBase2NDigitReader digitReader,
        ref TWriter digitWriter)
        where TWriter : struct, IBase2NDigitWriter, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(digitReader);
        if (digitReader.MoveNext())
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!digitReader.MoveNext())
                {
                    digitWriter.WriteDigit(last, -digitReader.CurrentBitCount);
                    break;
                }
                digitWriter.WriteDigit(last);
                last = digitReader.Current;
            }
        }
    }
    public static void Process<TReader>(
        ref TReader digitReader,
        IBase2NDigitWriter digitWriter)
        where TReader : struct, IBase2NDigitReader, allows ref struct
    {
        ArgumentNullException.ThrowIfNull(digitWriter);
        if (digitReader.MoveNext())
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!digitReader.MoveNext())
                {
                    digitWriter.WriteDigit(last, -digitReader.CurrentBitCount);
                    break;
                }
                digitWriter.WriteDigit(last);
                last = digitReader.Current;
            }
        }
    }
    public static void Process<TReader, TWriter>(
        ref TReader digitReader,
        ref TWriter digitWriter)
        where TReader : struct, IBase2NDigitReader, allows ref struct
        where TWriter : struct, IBase2NDigitWriter, allows ref struct
    {
        if (digitReader.MoveNext())
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!digitReader.MoveNext())
                {
                    digitWriter.WriteDigit(last, -digitReader.CurrentBitCount);
                    break;
                }
                digitWriter.WriteDigit(last);
                last = digitReader.Current;
            }
        }
    }
    public static async ValueTask ProcessAsync(
        IAsyncBase2NDigitReader digitReader,
        IBase2NDigitWriter digitWriter)
    {
        ArgumentNullException.ThrowIfNull(digitReader);
        ArgumentNullException.ThrowIfNull(digitWriter);
        if (await digitReader.MoveNextAsync().ConfigureAwait(false))
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!await digitReader.MoveNextAsync().ConfigureAwait(false))
                {
                    digitWriter.WriteDigit(last, -digitReader.CurrentBitCount);
                    break;
                }
                digitWriter.WriteDigit(last);
                last = digitReader.Current;
            }
        }
    }
    public static async ValueTask ProcessAsync(
        IBase2NDigitReader digitReader,
        IAsyncBase2NDigitWriter digitWriter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(digitReader);
        ArgumentNullException.ThrowIfNull(digitWriter);
        if (digitReader.MoveNext())
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!digitReader.MoveNext())
                {
                    await digitWriter.WriteDigitAsync(last, -digitReader.CurrentBitCount, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                }
                await digitWriter.WriteDigitAsync(last, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                last = digitReader.Current;
            }
        }
    }
    public static async ValueTask ProcessAsync(
        IAsyncBase2NDigitReader digitReader,
        IAsyncBase2NDigitWriter digitWriter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(digitReader);
        ArgumentNullException.ThrowIfNull(digitWriter);
        if (await digitReader.MoveNextAsync().ConfigureAwait(false))
        {
            int last = digitReader.Current;
            while (true)
            {
                if (!await digitReader.MoveNextAsync().ConfigureAwait(false))
                {
                    await digitWriter.WriteDigitAsync(last, -digitReader.CurrentBitCount, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                }
                await digitWriter.WriteDigitAsync(last, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                last = digitReader.Current;
            }
        }
    }
}