using Base2N.Encoders;
using Base2N.Text.Decoders;
using Base2N.Text.Dictionaries;
using Base2N.Text.Encoders;
using Base2N.Text.ExtraBitsHandlers;

namespace Base2N.Text;

public static class Base64Utils
{
    public const string DefaultDigits = OptimizedBase64Dictionary.OptimizedPrefix + "+/";
    public const string UrlDigits = OptimizedBase64Dictionary.OptimizedPrefix + "-_";

    public static RefCharDigitReader<OptimizedBase64Dictionary, Base64LikeExtraBitsHandler> CreateOptimizedRefReader(
        ReadOnlySpan<char> text,
        ReadOnlySpan<char> charSet,
        char padding)
        => new(text, OptimizedBase64Dictionary.Create(charSet), new(padding));
    public static RefCharDigitReader<OptimizedBase64Dictionary, NullExtraBitsHandler> CreateOptimizedRefReaderWithoutPadding(
        ReadOnlySpan<char> text,
        ReadOnlySpan<char> charSet)
        => new(text, OptimizedBase64Dictionary.Create(charSet), default);
    public static RefCharDigitReader<OptimizedBase64Dictionary, Base64LikeExtraBitsHandler> CreateOptimizedBase64RefReader(
        ReadOnlySpan<char> text)
        => CreateOptimizedRefReader(text, DefaultDigits, '=');
    public static RefCharDigitReader<OptimizedBase64Dictionary, NullExtraBitsHandler> CreateOptimizedBase64UrlRefReader(
        ReadOnlySpan<char> text)
        => CreateOptimizedRefReaderWithoutPadding(text, UrlDigits);
    public static CharDigitReader<OptimizedBase64Dictionary, Base64LikeExtraBitsHandler> CreateOptimizedReader(
        TextReader reader,
        ReadOnlySpan<char> charSet,
        char padding)
        => new(reader, OptimizedBase64Dictionary.Create(charSet), new(padding));
    public static CharDigitReader<OptimizedBase64Dictionary, NullExtraBitsHandler> CreateOptimizedReaderWithoutPadding(
        TextReader reader,
        ReadOnlySpan<char> charSet)
        => new(reader, OptimizedBase64Dictionary.Create(charSet), default);
    public static CharDigitReader<OptimizedBase64Dictionary, Base64LikeExtraBitsHandler> CreateOptimizedBase64Reader(
        TextReader reader)
        => CreateOptimizedReader(reader, DefaultDigits, '=');
    public static CharDigitReader<OptimizedBase64Dictionary, NullExtraBitsHandler> CreateOptimizedBase64UrlReader(TextReader reader)
        => CreateOptimizedReaderWithoutPadding(reader, UrlDigits);
    public static IBase2NDigitReader CreateReader(
        TextReader reader,
        ReadOnlySpan<char> charSet,
        char? padding)
    {
        if (charSet.Length == 64 && charSet.StartsWith(OptimizedBase64Dictionary.OptimizedPrefix))
        {
            return padding.HasValue ?
                CreateOptimizedReader(reader, charSet, padding.Value) :
                CreateOptimizedReaderWithoutPadding(reader, charSet);
        }
        else
        {
            return padding.HasValue ?
                DigitReaders.Create(reader, charSet, new Base64LikeExtraBitsHandler(padding.Value)) :
                DigitReaders.Create(reader, charSet, default(NullExtraBitsHandler));
        }
    }
    public static TextDigitWriter CreateWriter(
        TextWriter writer,
        string charSet,
        char? padding)
    {
        return CreateWriter(writer, charSet.AsMemory(), padding);
    }
    public static TextDigitWriter CreateWriter(
        TextWriter writer,
        char[] charSet,
        char? padding)
    {
        return CreateWriter(writer, charSet.AsMemory(), padding);
    }
    public static TextDigitWriter CreateWriter(
        TextWriter writer,
        ReadOnlyMemory<char> charSet,
        char? padding)
    {
        return padding.HasValue ?
            DigitWriters.Create(writer, charSet, new Base64LikeExtraBitsHandler(padding.Value)) :
            DigitWriters.Create(writer, charSet, default(NullExtraBitsHandler));
    }
}