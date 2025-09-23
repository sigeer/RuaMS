using Application.Shared.Net;
using BenchmarkDotNet.Attributes;
using System.Text;

namespace Application.Benchmark
{
    /// <summary>
    ///   | Method         | Mean     | Error   | StdDev  | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    ///   |--------------- |---------:|--------:|--------:|------:|--------:|-------:|----------:|------------:|
    ///   | WriteStringOld | 478.6 ns | 6.74 ns | 5.97 ns |  1.00 |    0.02 | 0.2851 |     448 B |        1.00 |
    ///   | WriteStringNew | 295.8 ns | 4.08 ns | 3.41 ns |  0.62 |    0.01 | 0.2294 |     360 B |        0.80 |
    /// </summary>
    [MemoryDiagnoser]
    public class PacketWriteStringBenchmark
    {
        public const string TestString = "It&apos;s a bowman town on a wide prairie, and you can choose to become a bowman here.";
        public PacketWriteStringBenchmark()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Benchmark(Baseline = true)]
        public void WriteStringOld()
        {
            var outPacket = new ByteBufOutPacket();
            outPacket.writeStringOld(TestString);
        }
        [Benchmark]
        public void WriteStringNew()
        {
            var outPacket = new ByteBufOutPacket();
            outPacket.WriteString(TestString);
        }
    }
    /// <summary>
    /// | Method            | Mean     | Error   | StdDev   | Median   | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
    /// |------------------ |---------:|--------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
    /// | WriteFixStringOld | 499.3 ns | 8.24 ns | 17.73 ns | 495.1 ns |  1.00 |    0.05 | 0.3262 |     512 B |        1.00 |
    /// | WriteFixStringNew | 317.4 ns | 8.10 ns | 23.11 ns | 311.1 ns |  0.64 |    0.05 | 0.2294 |     360 B |        0.70 |
    /// </summary>
    [MemoryDiagnoser]
    public class PacketWriteFixStringBenchmark
    {
        public const string TestString = "It&apos;s a bowman town on a wide prairie, and you can choose to become a bowman here.";
        public PacketWriteFixStringBenchmark()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Benchmark(Baseline = true)]
        public void WriteFixStringOld()
        {
            var outPacket = new ByteBufOutPacket();
            outPacket.writeFixedStringOld(TestString);
        }
        [Benchmark]
        public void WriteFixStringNew()
        {
            var outPacket = new ByteBufOutPacket();
            outPacket.WriteFixedString(TestString);
        }
    }
}
