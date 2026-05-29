using Base2N.Encoders;
using Base2N.Text.ExtraBitsHandlers;
using System.Collections;

namespace Base2N.Text.Decoders;

public sealed class CodePointDigitReader<TDictionary, TExtraBitsHandler>
    : IBase2NDigitReader
    where TDictionary : IReadOnlyDictionary<int, int>
    where TExtraBitsHandler : IExtraBitsHandler
{
    private readonly bool _leaveOpen;
    private bool _disposed;
    public TDictionary ItemDictionary { get; }
    public TExtraBitsHandler ExtraBitsHandler { get; }
    public TextReader BaseReader { get; }
    public int CurrentBitCount { get; private set; }
    public int Current { get; private set; }
    object IEnumerator.Current => Current;

    public CodePointDigitReader(TextReader reader, TDictionary itemDictionary, TExtraBitsHandler extraBitsHandler, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull<TDictionary>(itemDictionary);
        ArgumentNullException.ThrowIfNull<TExtraBitsHandler>(extraBitsHandler);
        BaseReader = reader;
        ItemDictionary = itemDictionary;
        ExtraBitsHandler = extraBitsHandler;
        _leaveOpen = leaveOpen;
    }

    public bool MoveNext()
    {
        // DO NOT mix using surrogate characters and non-BMP code points!
        int extraBits = ExtraBitsHandler.TryReadExtraBits(BaseReader);
        if (extraBits >= 0)
        {
            CurrentBitCount = -extraBits;
            return false;
        }
        else if (BaseReader.Peek() is int read and >= 0)
        {
            int digit;
            if (char.IsHighSurrogate((char)read))
            {
                if (ItemDictionary.TryGetValue(read, out digit))
                {
                    BaseReader.Read();
                    Current = digit;
                    return true;
                }
                int highSurrogate = read;
                BaseReader.Read();
                if (BaseReader.Peek() is int lowSurrogateRead and >= 0 && char.IsLowSurrogate((char)lowSurrogateRead))
                    read = char.ConvertToUtf32((char)highSurrogate, (char)lowSurrogateRead);
                else
                {
                    // NOTE:
                    // If the surrogate pair is invalid and the high surrogate is not in the dictionary,
                    // it will still consume the high surrogate character because TextReader cannot peek 2 characters.
                    CurrentBitCount = 0;
                    return false;
                }
            }
            if (ItemDictionary.TryGetValue(read, out digit))
            {
                BaseReader.Read();
                Current = digit;
                return true;
            }
        }
        CurrentBitCount = 0;
        return false;
    }
    public void Reset()
        => throw new NotSupportedException();
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (!_leaveOpen)
                BaseReader.Dispose();
        }
    }
}