using System.Numerics;

namespace Base2N.Text.ExtraBitsHandlers;

public readonly struct NullExtraBitsHandler
    : IExtraBitsHandler, IEquatable<NullExtraBitsHandler>, IEqualityOperators<NullExtraBitsHandler, NullExtraBitsHandler, bool>
{
    public int TryReadExtraBits(scoped ReadOnlySpan<char> text, ref int position)
        => -1;
    public int TryReadExtraBits(TextReader reader)
        => -1;
    public ValueTask<int> TryReadExtraBitsAsync(TextReader reader, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(-1);
    public void WriteExtraBits(TextWriter writer, int extraBits) { }
    public ValueTask WriteExtraBitsAsync(TextWriter writer, int extraBits, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public bool Equals(NullExtraBitsHandler other)
        => true;
    public override bool Equals(object? obj)
        => obj is NullExtraBitsHandler;
    public override int GetHashCode()
        => 0;
    public static bool operator ==(NullExtraBitsHandler left, NullExtraBitsHandler right)
        => true;
    public static bool operator !=(NullExtraBitsHandler left, NullExtraBitsHandler right)
        => false;
}