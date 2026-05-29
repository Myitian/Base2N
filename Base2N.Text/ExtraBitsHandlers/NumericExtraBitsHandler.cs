using System.Buffers;
using System.Numerics;

namespace Base2N.Text.ExtraBitsHandlers;

public readonly struct NumericExtraBitsHandler(char seperator, SimpleNumberFormat format)
    : IExtraBitsHandler, IEquatable<NumericExtraBitsHandler>, IEqualityOperators<NumericExtraBitsHandler, NumericExtraBitsHandler, bool>
{
    public readonly char Seperator { get; } = seperator;
    public readonly SimpleNumberFormat Format { get; } = format switch
    {
        SimpleNumberFormat.Binary or SimpleNumberFormat.UppercaseHexadecimal or SimpleNumberFormat.LowercaseHexadecimal => format,
        _ => SimpleNumberFormat.Decimal
    };

    public static NumericExtraBitsHandler Default { get; } = new(':', SimpleNumberFormat.Decimal);

    public int TryReadExtraBits(scoped ReadOnlySpan<char> text, ref int position)
    {
        if (position >= text.Length)
            return 0;
        if (text[position] != Seperator)
            return -1;
        position++;
        int extraBits = 0;
        int radix = Format switch
        {
            SimpleNumberFormat.Binary => 2,
            SimpleNumberFormat.Decimal => 10,
            _ => 16
        };
        while (position < text.Length)
        {
            if (!TryGetDigitValue(text[position], radix, out int value))
                break;
            extraBits = extraBits * radix + value;
            position++;
        }
        return extraBits;
    }
    public int TryReadExtraBits(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        int read = reader.Peek();
        if (read < 0)
            return 0;
        if (read != Seperator)
            return -1;
        int extraBits = 0;
        int radix = Format switch
        {
            SimpleNumberFormat.Binary => 2,
            SimpleNumberFormat.Decimal => 10,
            _ => 16
        };
        while (true)
        {
            if (!TryGetDigitValue(read, radix, out int value))
                break;
            extraBits = extraBits * radix + value;
            reader.Read();
            read = reader.Peek();
        }
        return extraBits;
    }
    public ValueTask<int> TryReadExtraBitsAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(TryReadExtraBits(reader));
    }
    public void WriteExtraBits(TextWriter writer, int extraBits)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(extraBits);
        char format = (char)Format;
        Span<char> span = stackalloc char[11];
        span[0] = Seperator;
        extraBits.TryFormat(span[1..], out int w, new ReadOnlySpan<char>(in format), null);
        writer.Write(span[..(w + 1)]);
    }
    public async ValueTask WriteExtraBitsAsync(TextWriter writer, int extraBits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(extraBits);
        char[] buffer = ArrayPool<char>.Shared.Rent(16);
        try
        {
            char format = (char)Format;
            Span<char> span = buffer;
            span[0] = Seperator;
            extraBits.TryFormat(span[1..], out int w, new ReadOnlySpan<char>(in format), null);
            await writer.WriteAsync(buffer.AsMemory(0, w + 1), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
    public static bool TryGetDigitValue(int read, int radix, out int value)
    {
        if (radix <= 10)
            value = read - '0';
        else if (read >= 'a')
            value = read - 'a';
        else if (read >= 'A')
            value = read - 'A';
        else
            value = read - '0';
        return value >= 0 && value < radix;
    }

    public bool Equals(NumericExtraBitsHandler other)
        => Seperator == other.Seperator && Format == other.Format;
    public override bool Equals(object? obj)
        => obj is NumericExtraBitsHandler other && Equals(other);
    public override int GetHashCode()
        => (Seperator << 16) | (int)Format;
    public static bool operator ==(NumericExtraBitsHandler left, NumericExtraBitsHandler right)
        => left.Equals(right);
    public static bool operator !=(NumericExtraBitsHandler left, NumericExtraBitsHandler right)
        => !left.Equals(right);
}
#pragma warning disable CA1028,CA1720
public enum SimpleNumberFormat : ushort
{
    Default,
    Binary = 'B',
    Decimal = 'D',
    UppercaseHexadecimal = 'X',
    LowercaseHexadecimal = 'x'
}