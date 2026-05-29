using Base2N.Encoders;
using Base2N.Text.ExtraBitsHandlers;
using System.Collections;

namespace Base2N.Text.Decoders;

public sealed class CharDigitReader<TDictionary, TExtraBitsHandler>
    : IBase2NDigitReader
    where TDictionary : IReadOnlyDictionary<char, int>
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

    public CharDigitReader(TextReader reader, TDictionary itemDictionary, TExtraBitsHandler extraBitsHandler, bool leaveOpen = false)
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
        int extraBits = ExtraBitsHandler.TryReadExtraBits(BaseReader);
        if (extraBits >= 0)
        {
            CurrentBitCount = -extraBits;
            return false;
        }
        else if (BaseReader.Peek() is int read and >= 0 && ItemDictionary.TryGetValue((char)read, out int digit))
        {
            BaseReader.Read();
            Current = digit;
            return true;
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