using Base2N.Decoders;
using Base2N.Text.ExtraBitsHandlers;

namespace Base2N.Text.Encoders;

public abstract class TextDigitWriter<TExtraBitsHandler> : TextDigitWriter
    where TExtraBitsHandler : IExtraBitsHandler
{
#pragma warning disable CA1051 // TExtraBitsHandler can be a struct
    protected readonly TExtraBitsHandler _extraBitsHandler;
#pragma warning restore CA1051
    protected TextDigitWriter(TextWriter writer, TExtraBitsHandler extraBitsHandler, bool leaveOpen = false)
        : base(writer, leaveOpen)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (extraBitsHandler is null)
            throw new ArgumentNullException(nameof(extraBitsHandler));
        _extraBitsHandler = extraBitsHandler;
    }
    protected void WriteExtraBits(int extraBits)
    {
        if (extraBits >= 0)
            _extraBitsHandler.WriteExtraBits(Writer, extraBits);
    }
    protected async ValueTask WriteExtraBitsAsync(int extraBits, CancellationToken cancellationToken = default)
    {
        if (extraBits >= 0)
            await _extraBitsHandler.WriteExtraBitsAsync(Writer, extraBits, cancellationToken)
                .ConfigureAwait(false);
    }
}
public abstract class TextDigitWriter
    : IBase2NDigitWriter, IAsyncBase2NDigitWriter
{
    protected TextWriter Writer { get; }
    protected bool LeaveOpen { get; }
    protected bool Disposed { get; set; }
    protected TextDigitWriter(TextWriter writer, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(writer);
        Writer = writer;
        LeaveOpen = leaveOpen;
    }
    public abstract void WriteDigit(int digit, int extraBits = -1);
    public abstract ValueTask WriteDigitAsync(int digit, int extraBits = -1, CancellationToken cancellationToken = default);
    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            Disposed = true;
            if (disposing && !LeaveOpen)
                Writer.Dispose();
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public async ValueTask DisposeAsync()
    {
        if (!Disposed)
        {
            Disposed = true;
            if (!LeaveOpen)
                await Writer.DisposeAsync().ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
    ~TextDigitWriter()
    {
        Dispose(false);
    }
}