namespace Base2N.Text.ExtraBitsHandlers;

public interface IExtraBitsHandler
{
    public static NullExtraBitsHandler Null => default; // NullExtraBitsHandler doesn't have any state, so we can just return default
    public static Base64LikeExtraBitsHandler Base64 => Base64LikeExtraBitsHandler.Default;
    public static SimpleExtraBitsHandler DefaultSimple => SimpleExtraBitsHandler.Default;
    public static NumericExtraBitsHandler DefaultNumeric => NumericExtraBitsHandler.Default;
    void WriteExtraBits(TextWriter writer, int extraBits);
    ValueTask WriteExtraBitsAsync(TextWriter writer, int extraBits, CancellationToken cancellationToken = default);
    int TryReadExtraBits(scoped ReadOnlySpan<char> text, ref int position);
    int TryReadExtraBits(TextReader reader);
    ValueTask<int> TryReadExtraBitsAsync(TextReader reader, CancellationToken cancellationToken = default);
}