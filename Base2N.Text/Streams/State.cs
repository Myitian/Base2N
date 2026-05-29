namespace Base2N.Text.Streams;

[Flags]
internal enum State
{
    Disposed = 0b0001,
    LeaveOpen = 0b0010,
    ReaderCompleted = 0b0100,
    EncoderCompleted = 0b1000,
    PooledBuffer = 0b0001_0000,
    ClearPooledBuffer = 0b0010_0000
}