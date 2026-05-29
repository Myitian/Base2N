using Base2N.Encoders;
using System.Collections;

namespace Base2N.ExampleCLI;

sealed class MeowDecoder : IBase2NDigitReader, IRadixGetter
{
    private readonly bool _leaveOpen;
    private bool _disposed;

    public int CurrentBitCount { get; private set; }
    public int Current { get; private set; }
    public TextReader BaseReader { get; }
    public int Radix { get; }
    object IEnumerator.Current => Current;

    public MeowDecoder(TextReader reader, int maxPhraseLength = 8, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(reader);
        Base2NUtils.ValidateRadix(maxPhraseLength);
        Radix = checked(maxPhraseLength * MeowCodec.PhraseSuffixList.Length);
        BaseReader = reader;
        _leaveOpen = leaveOpen;
    }

    public bool MoveNext()
    {
        int read = BaseReader.Peek();
        if (read is not MeowCodec.Miao)
            goto END;
        int mCount = -1;
        do
        {
            BaseReader.Read();
            read = BaseReader.Peek();
            mCount++;
        }
        while (read is MeowCodec.Miao);
        Span<char> suffixBuffer = stackalloc char[MeowCodec.MaxSuffixLength];
        int maxRead = suffixBuffer.Length - 1;
        int suffixCharCount = 0;
        while (suffixCharCount < maxRead && read is not (< 0
            or MeowCodec.TildeMark
            or MeowCodec.PeriodMark
            or MeowCodec.CommaMark
            or MeowCodec.SemicolonMark
            or MeowCodec.PauseMark
            or MeowCodec.ExclamationMark
            or MeowCodec.QuestionMark))
        {
            suffixBuffer[suffixCharCount++] = (char)read;
            BaseReader.Read();
            read = BaseReader.Peek();
        }
        if (read < 0)
            goto END;
        suffixBuffer[suffixCharCount++] = (char)read;
        int tCount = -1;
        if (read is not MeowCodec.TildeMark)
            BaseReader.Read();
        else
        {
            while (read is MeowCodec.TildeMark)
            {
                tCount++;
                BaseReader.Read();
                read = BaseReader.Peek();
            }
        }
        if (tCount > 0)
        {
            CurrentBitCount = -(mCount * 2 + tCount);
            return false;
        }
        if (!MeowCodec.PhraseSuffixMap
            .GetAlternateLookup<ReadOnlySpan<char>>()
            .TryGetValue(suffixBuffer[..suffixCharCount], out int suffixIndex))
            goto END;
        int digit = mCount * MeowCodec.PhraseSuffixList.Length + suffixIndex;
        Current = digit;
        return true;
    END:
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
            if (_leaveOpen)
                BaseReader.Dispose();
        }
    }
}