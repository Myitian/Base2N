using Base2N;
using Base2N.Decoders;
using Base2N.Encoders;
using System.Buffers;
using System.Security.Cryptography;

byte[] bytes = RandomNumberGenerator.GetBytes(77);
Console.WriteLine(Convert.ToBase64String(bytes));
MemoryBase2NEncoder b64 = new(bytes, 64);
Console.WriteLine($"{string.Concat(Enumerable.Range(0, b64.Count).Select(i =>
{
    int v = b64[i];
    return v switch
    {
        < 26 => (char)('A' + v),
        < 52 => (char)('a' + v - 26),
        < 62 => (char)('0' + v - 52),
        62 => '+',
        _ => '/'
    };
}))}:{b64.ExtraBits}");
Console.WriteLine($"{string.Concat(b64.Select(i => i switch
{
    < 26 => (char)('A' + i),
    < 52 => (char)('a' + i - 26),
    < 62 => (char)('0' + i - 52),
    62 => '+',
    _ => '/'
}))}:{b64.ExtraBits}");
using MemoryStream resultBuffer = new();
for (int i = 0; i < 200000; i++)
{
    Test1(resultBuffer);
    await Test2(resultBuffer);
    Test3(resultBuffer);
}

static void Test1(MemoryStream resultBuffer)
{
    resultBuffer.SetLength(0);
    int length = 1 + RandomNumberGenerator.GetInt32(1024);
    int radix = 1 << (2 + RandomNumberGenerator.GetInt32(29));
    byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
    try
    {
        Span<byte> span = buffer.AsSpan(0, length);
        RandomNumberGenerator.Fill(span);
        {
            SpanBase2NEncoder encoder = new(span, radix);
            using SpanBase2NEncoder.Enumerator enumerator = encoder.GetEnumerator();
            using PipeBase2NDecoder decoder = new(resultBuffer, radix, new(leaveOpen: true));
            if (enumerator.MoveNext())
            {
                int last = enumerator.Current;
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        decoder.WriteDigit(last, -enumerator.CurrentBits);
                        if (-enumerator.CurrentBits != encoder.ExtraBits)
                            throw new Exception("Test failed.");
                        break;
                    }
                    decoder.WriteDigit(last);
                    last = enumerator.Current;
                }
            }
        }
        if (!(resultBuffer.TryGetBuffer(out ArraySegment<byte> result) && span.SequenceEqual(result)))
            throw new Exception("Test failed.");
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
static async ValueTask Test2(MemoryStream resultBuffer)
{
    resultBuffer.SetLength(0);
    int length = 1 + RandomNumberGenerator.GetInt32(10);
    int radix = 1 << (2 + RandomNumberGenerator.GetInt32(29));
    byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
    try
    {
        Memory<byte> mem = buffer.AsMemory(0, length);
        RandomNumberGenerator.Fill(mem.Span);
        using (MemoryStream ms = new(buffer, 0, length))
        {
            PipeBase2NEncoder encoder = new(ms, radix);
            await using PipeBase2NEncoder.AsyncEnumerator enumerator = encoder.GetAsyncEnumerator();
            await using PipeBase2NDecoder decoder = new(resultBuffer, radix, new(leaveOpen: true));
            if (await enumerator.MoveNextAsync())
            {
                int last = enumerator.Current;
                while (true)
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        await decoder.WriteDigitAsync(last, -enumerator.CurrentBits);
                        if (-enumerator.CurrentBits != Base2NUtils.GetCountAndExtraBits(length, radix).ExtraBits)
                            throw new Exception("Test failed.");
                        break;
                    }
                    await decoder.WriteDigitAsync(last);
                    last = enumerator.Current;
                }
            }
        }
        if (!(resultBuffer.TryGetBuffer(out ArraySegment<byte> result) && mem.Span.SequenceEqual(result)))
            throw new Exception("Test failed.");
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
static void Test3(MemoryStream resultBuffer)
{
    resultBuffer.SetLength(0);
    int length = 1 + RandomNumberGenerator.GetInt32(1024);
    int radix = 1 << (2 + RandomNumberGenerator.GetInt32(29));
    byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
    try
    {
        Memory<byte> mem = buffer.AsMemory(0, length);
        RandomNumberGenerator.Fill(mem.Span);
        using (MemoryStream ms = new(buffer, 0, length))
        {
            SyncStreamBase2NEncoder encoder = new(ms, radix);
            using SyncStreamBase2NEncoder.Enumerator enumerator = encoder.GetEnumerator();
            using PipeBase2NDecoder decoder = new(resultBuffer, radix, new(leaveOpen: true));
            if (enumerator.MoveNext())
            {
                int last = enumerator.Current;
                while (true)
                {
                    if (!enumerator.MoveNext())
                    {
                        decoder.WriteDigit(last, -enumerator.CurrentBits);
                        if (-enumerator.CurrentBits != Base2NUtils.GetCountAndExtraBits(length, radix).ExtraBits)
                            throw new Exception("Test failed.");
                        break;
                    }
                    decoder.WriteDigit(last);
                    last = enumerator.Current;
                }
            }
        }
        if (!(resultBuffer.TryGetBuffer(out ArraySegment<byte> result) && mem.Span.SequenceEqual(result)))
            throw new Exception("Test failed.");
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}