using Base2N.Text.ExtraBitsHandlers;
using System.Buffers;
using System.Collections.Frozen;

namespace Base2N.Text.Decoders;

public static class DigitReaders
{
    public static CharDigitReader<TDictionary, TExtraBitsHandler> CreateCharDigitReader<TDictionary, TExtraBitsHandler>(
        TextReader reader,
        TDictionary dict,
        TExtraBitsHandler extraBitsHandler)
        where TDictionary : IReadOnlyDictionary<char, int>
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(reader, dict, extraBitsHandler);
    }
    public static CodePointDigitReader<TDictionary, TExtraBitsHandler> CreateCodePointDigitReader<TDictionary, TExtraBitsHandler>(
        TextReader reader,
        TDictionary dict,
        TExtraBitsHandler extraBitsHandler)
        where TDictionary : IReadOnlyDictionary<int, int>
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(reader, dict, extraBitsHandler);
    }
    public static RefCharDigitReader<TDictionary, TExtraBitsHandler> CreateRefCharDigitReader<TDictionary, TExtraBitsHandler>(
        ReadOnlySpan<char> text,
        TDictionary dict,
        TExtraBitsHandler extraBitsHandler)
        where TDictionary : IReadOnlyDictionary<char, int>, allows ref struct
        where TExtraBitsHandler : IExtraBitsHandler, allows ref struct
    {
        return new(text, dict, extraBitsHandler);
    }
    public static RefCodePointDigitReader<TDictionary, TExtraBitsHandler> CreateRefCodePointDigitReader<TDictionary, TExtraBitsHandler>(
        ReadOnlySpan<char> text,
        TDictionary dict,
        TExtraBitsHandler extraBitsHandler)
        where TDictionary : IReadOnlyDictionary<int, int>, allows ref struct
        where TExtraBitsHandler : IExtraBitsHandler, allows ref struct
    {
        return new(text, dict, extraBitsHandler);
    }

    public static CharDigitReader<FrozenDictionary<char, int>, TExtraBitsHandler> Create<TExtraBitsHandler>(
        TextReader reader,
        ReadOnlySpan<char> charSet,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler
    {
        FrozenDictionary<char, int> dict;
        KeyValuePair<char, int>[] buffer = ArrayPool<KeyValuePair<char, int>>.Shared.Rent(charSet.Length);
        try
        {
            for (int i = 0; i < charSet.Length; i++)
                buffer[i] = new(charSet[i], i);
            dict = buffer.ToFrozenDictionary();
        }
        finally
        {
            ArrayPool<KeyValuePair<char, int>>.Shared.Return(buffer);
        }
        return new(reader, dict, extraBitsHandler);
    }
    public static RefCharDigitReader<FrozenDictionary<char, int>, TExtraBitsHandler> Create<TExtraBitsHandler>(
        ReadOnlySpan<char> text,
        ReadOnlySpan<char> charSet,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler, allows ref struct
    {
        FrozenDictionary<char, int> dict;
        KeyValuePair<char, int>[] buffer = ArrayPool<KeyValuePair<char, int>>.Shared.Rent(charSet.Length);
        try
        {
            for (int i = 0; i < charSet.Length; i++)
                buffer[i] = new(charSet[i], i);
            dict = buffer.ToFrozenDictionary();
        }
        finally
        {
            ArrayPool<KeyValuePair<char, int>>.Shared.Return(buffer);
        }
        return new(text, dict, extraBitsHandler);
    }
}