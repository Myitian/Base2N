using Base2N.Encoders;
using Base2N.Text.ExtraBitsHandlers;
using System.Buffers;
using System.Collections;
using System.Text;

namespace Base2N.Text.Decoders;

public ref struct RefCodePointDigitReader<TDictionary, TExtraBitsHandler>
    : IBase2NDigitReader
    where TDictionary : IReadOnlyDictionary<int, int>, allows ref struct
    where TExtraBitsHandler : IExtraBitsHandler, allows ref struct
{
    private int _position;
    public readonly TDictionary ItemDictionary { get; }
    public readonly TExtraBitsHandler ExtraBitsHandler { get; }
    public readonly ReadOnlySpan<char> Text { get; }
    public int Position { readonly get => _position; set => _position = Math.Max(value, 0); }
    public int CurrentBitCount { readonly get; private set; }
    public int Current { readonly get; private set; }
    readonly object IEnumerator.Current => Current;

    public RefCodePointDigitReader(ReadOnlySpan<char> text, TDictionary itemDictionary, TExtraBitsHandler extraBitsHandler)
    {
        ArgumentNullException.ThrowIfNull(itemDictionary);
        ArgumentNullException.ThrowIfNull(extraBitsHandler);
        Text = text;
        ItemDictionary = itemDictionary;
        ExtraBitsHandler = extraBitsHandler;
    }

    public bool MoveNext()
    {
        if (Position < Text.Length)
        {
            int extraBits = ExtraBitsHandler.TryReadExtraBits(Text, ref _position);
            if (extraBits >= 0)
            {
                CurrentBitCount = -extraBits;
                return false;
            }
            else
            {
                OperationStatus status = Rune.DecodeFromUtf16(Text[_position..], out Rune rune, out int charCount);
                int codepoint;
                if (status is OperationStatus.Done)
                    codepoint = rune.Value;
                else
                {
                    codepoint = Text[_position];
                    charCount = 1;
                }
                if (ItemDictionary.TryGetValue(codepoint, out int digit))
                {
                    _position += charCount;
                    Current = digit;
                    return true;
                }
            }
        }
        CurrentBitCount = 0;
        return false;
    }
    public void Reset()
    {
        _position = 0;
        CurrentBitCount = 0;
    }
    public readonly void Dispose() { }
}