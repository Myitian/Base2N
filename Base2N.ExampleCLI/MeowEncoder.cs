using Base2N.Decoders;

namespace Base2N.ExampleCLI;

class MeowEncoder : IBase2NDigitWriter, IRadixGetter
{
    private readonly bool _leaveOpen;
    private bool _disposed;

    public TextWriter BaseWriter { get; }
    public int Radix { get; }

    public MeowEncoder(TextWriter writer, int maxPhraseLength = 8, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(writer);
        Base2NUtils.ValidateRadix(maxPhraseLength);
        Radix = checked(maxPhraseLength * MeowCodec.PhraseSuffixList.Length);
        BaseWriter = writer;
        _leaveOpen = leaveOpen;
    }
    public void WriteDigit(int digit, int extraBits = -1)
    {
        if (extraBits != int.MaxValue)
        {
            (int phraseLength, int suffixIndex) = Math.DivRem(digit, MeowCodec.PhraseSuffixList.Length);
            string suffix = MeowCodec.PhraseSuffixList[suffixIndex];
            Span<char> buffer = stackalloc char[++phraseLength + suffix.Length];
            buffer[..phraseLength].Fill(MeowCodec.Miao);
            suffix.CopyTo(buffer[phraseLength..]);
            BaseWriter.Write(buffer);
        }
        if (extraBits >= 0)
        {
            (int mLength, int tLength) = Math.DivRem(extraBits, 2);
            Span<char> buffer = stackalloc char[++mLength + ++tLength];
            buffer[..mLength].Fill(MeowCodec.Miao);
            buffer[mLength..].Fill(MeowCodec.TildeMark);
            BaseWriter.Write(buffer);
        }
    }
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_leaveOpen)
                BaseWriter.Dispose();
        }
    }
}