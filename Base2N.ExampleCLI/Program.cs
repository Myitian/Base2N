using Base2N.Streams;
using System.Text;

namespace Base2N.ExampleCLI;

static class Program
{
    static void Main(string[] args)
    {
        if (args is [string mode, ..])
        {
            if ("encode".StartsWith(mode, StringComparison.OrdinalIgnoreCase))
            {
                Encode();
                return;
            }
            else if ("decode".StartsWith(mode, StringComparison.OrdinalIgnoreCase))
            {
                Decode();
                return;
            }
            else
                Console.Error.WriteLine("Unknown mode.");
        }
        Console.Error.WriteLine("""
            Usage: Base2N.ExampleCLI <mode>
            Modes:
              encode - Encode standard input to standard output using MeowCodec
              decode - Decode standard input to standard output using MeowCodec
            Example:
              echo "Hello, world!" | Base2N.ExampleCLI encode
            """);
    }
    public static void Encode()
    {
        Console.OutputEncoding = Encoding.UTF8;
        using MeowEncoder encoder = new(Console.Out);
        using Stream writer = Base2NEncoderStream.Create(encoder, encoder.Radix);
        using Stream stdin = Console.OpenStandardInput();
        stdin.CopyTo(writer);
    }
    public static void Decode()
    {
        Console.InputEncoding = Encoding.UTF8;
        using MeowDecoder decoder = new(Console.In);
        using Stream reader = Base2NDecoderStream.Create(decoder, decoder.Radix);
        using Stream stdout = Console.OpenStandardOutput();
        reader.CopyTo(stdout);
    }
}