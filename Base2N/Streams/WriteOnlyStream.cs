using System.Runtime.ExceptionServices;

namespace Base2N.Streams;

public abstract class WriteOnlyStream : NonSeekableStream
{
    public override bool CanRead => false;
    public override bool CanWrite => true;

    public override void CopyTo(Stream destination, int bufferSize)
        => throw new NotSupportedException();
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => Task.FromException<int>(ExceptionDispatchInfo.SetCurrentStackTrace(new NotSupportedException()));
    public override int Read(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();
    public override int Read(Span<byte> buffer)
        => throw new NotSupportedException();
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        => Task.FromException<int>(ExceptionDispatchInfo.SetCurrentStackTrace(new NotSupportedException()));
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => ValueTask.FromException<int>(ExceptionDispatchInfo.SetCurrentStackTrace(new NotSupportedException()));
    public override int ReadByte()
        => throw new NotSupportedException();
}