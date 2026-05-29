using Base2N.Text.DigitCollections;
using Base2N.Text.ExtraBitsHandlers;

namespace Base2N.Text.Encoders;

public static class DigitWriters
{
    public static CharDigitWriter<TDigitCollection, TExtraBitsHandler> CreateCharDigitWriter<TDigitCollection, TExtraBitsHandler>(
        TextWriter writer,
        TDigitCollection digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TDigitCollection : IDigitCollection<char>
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, digitCollection, extraBitsHandler);
    }
    public static CodePointDigitWriter<TDigitCollection, TExtraBitsHandler> CreateCodePointDigitWriter<TDigitCollection, TExtraBitsHandler>(
        TextWriter writer,
        TDigitCollection digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TDigitCollection : IDigitCollection<int>
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, digitCollection, extraBitsHandler);
    }
    public static StringDigitWriter<TDigitCollection, TExtraBitsHandler> CreateStringDigitWriter<TDigitCollection, TExtraBitsHandler>(
        TextWriter writer,
        TDigitCollection digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TDigitCollection : IDigitCollection<string>
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, digitCollection, extraBitsHandler);
    }

    public static CharDigitWriter<CharDigitCollection, TExtraBitsHandler> Create<TExtraBitsHandler>(
        TextWriter writer,
        ReadOnlyMemory<char> digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, new(digitCollection), extraBitsHandler);
    }
    public static CharDigitWriter<CharDigitCollection, TExtraBitsHandler> Create<TExtraBitsHandler>(
        TextWriter writer,
        char[] digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, new(digitCollection), extraBitsHandler);
    }
    public static CharDigitWriter<CharDigitCollection, TExtraBitsHandler> Create<TExtraBitsHandler>(
        TextWriter writer,
        string digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, new(digitCollection), extraBitsHandler);
    }
    public static StringDigitWriter<StringDigitCollection, TExtraBitsHandler> Create<TExtraBitsHandler>(
        TextWriter writer,
        ReadOnlyMemory<string> digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, new(digitCollection), extraBitsHandler);
    }
    public static StringDigitWriter<StringDigitCollection, TExtraBitsHandler> Create<TExtraBitsHandler>(
        TextWriter writer,
        string[] digitCollection,
        TExtraBitsHandler extraBitsHandler)
        where TExtraBitsHandler : IExtraBitsHandler
    {
        return new(writer, new(digitCollection), extraBitsHandler);
    }
}