using System.Runtime.ExceptionServices;

namespace Base2N.Streams;

public abstract class ReadOnlyStream : NonSeekableStream
{
    public override bool CanRead => true;
    public override bool CanWrite => false;
    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    public override void Write(ReadOnlySpan<byte> buffer)
        => throw new NotSupportedException();
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        => Task.FromException(ExceptionDispatchInfo.SetCurrentStackTrace(new NotSupportedException()));
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        => ValueTask.FromException(ExceptionDispatchInfo.SetCurrentStackTrace(new NotSupportedException()));
    public override void WriteByte(byte value)
        => throw new NotSupportedException();
    public override void Flush() { }
    public override Task FlushAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}