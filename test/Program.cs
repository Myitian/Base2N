using Base2N;
using Base2N.Decoders;
using Base2N.Encoders;
using Base2N.Streams;
using Base2N.Text;
using Base2N.Text.Decoders;
using Base2N.Text.Dictionaries;
using Base2N.Text.DigitCollections;
using Base2N.Text.Encoders;
using Base2N.Text.ExtraBitsHandlers;
using Base2N.Text.Streams;
using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

TestText();
byte[] bytes = RandomNumberGenerator.GetBytes(77);
Console.WriteLine(Convert.ToBase64String(bytes));
MemoryBase2NEncoder b64 = new(bytes, 64);
Console.WriteLine($"{string.Concat(Enumerable.Range(0, b64.Count).Select(i =>
{
    int d = b64[i];
    return d switch
    {
        < 26 => (char)('A' + d),
        < 52 => (char)('a' + d - 26),
        < 62 => (char)('0' + d - 52),
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
for (int i = 0; i < 100000; i++)
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
            SpanBase2NEncoder.Enumerator enumerator = encoder.GetEnumerator();
            using PipeBase2NDecoder decoder = new(resultBuffer, radix, new(leaveOpen: true));
            Base2NProcessor.Process(ref enumerator, decoder);
            if (-enumerator.CurrentBitCount != encoder.ExtraBits)
                throw new Exception("Test failed.");
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
            await using PipeBase2NEncoder.Enumerator enumerator = encoder.GetAsyncEnumerator();
            await using PipeBase2NDecoder decoder = new(resultBuffer, radix, new(leaveOpen: true));
            await Base2NProcessor.ProcessAsync(enumerator, decoder);
            if (-enumerator.CurrentBitCount != Base2NUtils.GetCountAndExtraBits(length, radix).ExtraBits)
                throw new Exception("Test failed.");
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
            StreamBase2NEncoder encoder = new(ms, radix);
            using StreamBase2NEncoder.Enumerator enumerator = encoder.GetEnumerator();
            using PipeBase2NDecoder decoder = new(resultBuffer, radix, new(leaveOpen: true));
            Base2NProcessor.Process(enumerator, decoder);
            if (-enumerator.CurrentBitCount != Base2NUtils.GetCountAndExtraBits(length, radix).ExtraBits)
                throw new Exception("Test failed.");
        }
        if (!(resultBuffer.TryGetBuffer(out ArraySegment<byte> result) && mem.Span.SequenceEqual(result)))
            throw new Exception("Test failed.");
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
static void TestText()
{

    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;
    string message = "Hello, World!\n你好，世界！\n\0\uFFFF\U0010FFFF";
    byte[] bMsg = Encoding.UTF8.GetBytes(message);
    Console.WriteLine(message);
    StringBuilder sb = new();
    {
        // Pull-based encoder + push-based writer
        using StreamBase2NEncoder.Enumerator tokenGen = new StreamBase2NEncoder(new StringReaderStream(message), 64).GetEnumerator();
        using IBase2NDigitWriter textGen = Base64Utils.CreateWriter(new StringWriter(sb), Base64Utils.DefaultDigits, '=');
        Base2NProcessor.Process(tokenGen, textGen);
    }
    string b64a = sb.ToString();
    Console.WriteLine(b64a);
    sb.Clear();
    {
        // Pure push-based convertion chain
        using StreamWriter sw = new(
            Base2NEncoderStream.CreateAsync(
                Base64Utils.CreateWriter(
                    new StringWriter(sb),
                    Base64Utils.DefaultDigits,
                    '='),
                64));
        sw.Write(message);
    }
    string b64b = sb.ToString();
    Console.WriteLine(b64b);
    string b64c = Convert.ToBase64String(bMsg);
    Console.WriteLine(b64c);
    Debug.Assert(b64a == b64b && b64a == b64c, "Error: Encoded results do not match!");
    sb.Clear();
    {
        // TextReader-based decoder
        using IBase2NDigitReader decoder = Base64Utils.CreateReader(new StringReader(b64a), Base64Utils.DefaultDigits, '=');
        using StreamBase2NDecoder encoder = new(new TextWriterStream(new StringWriter(sb)), 64);
        Base2NProcessor.Process(decoder, encoder);
    }
    string msg1 = sb.ToString();
    sb.Clear();
    {
        // ReadOnlySpan-based decoder
        var decoder = Base64Utils.CreateOptimizedBase64RefReader(b64a);
        using StreamBase2NDecoder encoder = new(new TextWriterStream(new StringWriter(sb)), 64);
        Base2NProcessor.Process(ref decoder, encoder);
    }
    string msg2 = sb.ToString();
    Debug.Assert(message == msg1 && message == msg2, "Error: Decoded results do not match!");
    using Aes aes = Aes.Create();
    aes.GenerateKey();
    aes.GenerateIV();
    Console.WriteLine($"""
    Key: {Convert.ToHexString(aes.Key)}
    IV: {Convert.ToHexString(aes.IV)}
    """);
    sb.Clear();
    {
        using ICryptoTransform transform = aes.CreateEncryptor();
        using StreamWriter sw = new(
            new BrotliStream(
                new CryptoStream(
                    Base2NEncoderStream.CreateAsync(
                        DigitWriters.CreateCodePointDigitWriter(
                            new StringWriter(sb),
                            new ContinuousCodePointDigitCollection('\xFFFF'),
                            IExtraBitsHandler.DefaultNumeric),
                        32),
                    transform,
                    CryptoStreamMode.Write),
                CompressionLevel.SmallestSize));
        sw.Write(message);
    }
    string result1 = sb.ToString();
    Console.WriteLine(result1);
    sb.Clear();
    {
        using ICryptoTransform transform = aes.CreateDecryptor();
        using BrotliStream br = new(
            new CryptoStream(
                Base2NDecoderStream.Create(
                    DigitReaders.CreateCodePointDigitReader(
                        new StringReader(result1),
                        new ContinuousCodePointDictionary('\xFFFF', 32),
                        IExtraBitsHandler.DefaultNumeric),
                    32),
                transform,
                CryptoStreamMode.Read),
            CompressionMode.Decompress);
        using MemoryStream ms = new();
        br.CopyTo(ms);
        Debug.Assert(bMsg.SequenceEqual(ms.ToArray()), "Error: Decoded results do not match!");
    }
}