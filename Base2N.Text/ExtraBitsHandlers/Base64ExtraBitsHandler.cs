using System.Buffers;
using System.Numerics;

namespace Base2N.Text.ExtraBitsHandlers;

public readonly struct Base64LikeExtraBitsHandler(char padding)
    : IExtraBitsHandler, IEquatable<Base64LikeExtraBitsHandler>, IEqualityOperators<Base64LikeExtraBitsHandler, Base64LikeExtraBitsHandler, bool>
{
    public readonly char Padding { get; } = padding;
    public static Base64LikeExtraBitsHandler Default { get; } = new('=');

    public int TryReadExtraBits(scoped ReadOnlySpan<char> text, ref int position)
    {
        if (position >= text.Length)
            return 0;
        int paddingCount = text[position..].IndexOfAnyExcept(Padding);
        paddingCount = paddingCount < 0 ? text.Length - position : paddingCount;
        position += paddingCount;
        return paddingCount == 0 ? -1 : paddingCount * 2;
    }
    public int TryReadExtraBits(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        int read = reader.Peek();
        if (read < 0)
            return 0;
        if (read != Padding)
            return -1;
        int paddingCount = 0;
        do
        {
            reader.Read();
            paddingCount++;
        }
        while (reader.Peek() == Padding);
        return paddingCount * 2;
    }
    public ValueTask<int> TryReadExtraBitsAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(TryReadExtraBits(reader));
    }
    public void WriteExtraBits(TextWriter writer, int extraBits)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(extraBits);
        int count = extraBits / 2;
        switch (count)
        {
            case 0:
                break;
            case 1:
                writer.Write(Padding);
                break;
            default:
                Span<char> buffer = stackalloc char[Math.Min(count, 16)];
                buffer.Fill(Padding);
                while (count > 0)
                {
                    writer.Write(buffer[..Math.Min(count, buffer.Length)]);
                    count -= buffer.Length;
                }
                break;
        }
    }
    public async ValueTask WriteExtraBitsAsync(TextWriter writer, int extraBits, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(extraBits);
        int count = extraBits / 2;
        switch (count)
        {
            case 0:
                break;
            case 1:
                await writer.WriteAsync(Padding).ConfigureAwait(false);
                break;
            default:
                char[] array = ArrayPool<char>.Shared.Rent(16);
                array.AsSpan().Fill(Padding);
                try
                {
                    ReadOnlyMemory<char> buffer = array.AsMemory();
                    while (count > 0)
                    {
                        await writer.WriteAsync(buffer[..Math.Min(count, buffer.Length)], cancellationToken)
                            .ConfigureAwait(false);
                        count -= buffer.Length;
                    }
                }
                finally
                {
                    ArrayPool<char>.Shared.Return(array);
                }
                break;
        }
    }

    public bool Equals(Base64LikeExtraBitsHandler other)
        => Padding == other.Padding;
    public override bool Equals(object? obj)
        => obj is Base64LikeExtraBitsHandler other && Equals(other);
    public override int GetHashCode()
        => Padding;
    public static bool operator ==(Base64LikeExtraBitsHandler left, Base64LikeExtraBitsHandler right)
        => left.Equals(right);
    public static bool operator !=(Base64LikeExtraBitsHandler left, Base64LikeExtraBitsHandler right)
        => !left.Equals(right);
}