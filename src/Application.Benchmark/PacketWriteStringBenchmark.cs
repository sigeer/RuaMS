using Application.Shared.Net;
using Application.Utility;
using BenchmarkDotNet.Attributes;
using DotNetty.Buffers;
using Microsoft.AspNetCore.Http;
using System;
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

    /// <summary>
    /// | Method        | Mean     | Error     | StdDev    | Median   | Ratio | RatioSD | Allocated | Alloc Ratio |
    /// |-------------- |---------:|----------:|----------:|---------:|------:|--------:|----------:|------------:|
    /// | ReadStringOld | 6.339 us | 1.1577 us | 3.1693 us | 5.100 us |  1.19 |    0.74 |     648 B |        1.00 |
    /// | ReadStringNew | 3.540 us | 0.2180 us | 0.6005 us | 3.300 us |  0.66 |    0.26 |     312 B |        0.48 |
    /// </summary>
    [MemoryDiagnoser]
    public class PacketReadStringBenchmark
    {
        public const string TestString = "It&apos;s a bowman town on a wide prairie, and you can choose to become a bowman here.";
        ByteBufInPacket reader;
        public PacketReadStringBenchmark()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }


        [IterationSetup]
        public void IterationSetup()
        {
            var writer = new ByteBufOutPacket();
            writer.writeString(TestString);

            reader = new ByteBufInPacket(writer.getBytes());
        }

        [Benchmark(Baseline = true)]
        public void ReadStringOld()
        {
            reader.seek(0);
            reader.readStringOld();
        }


        [Benchmark]
        public void ReadStringNew()
        {
            reader.seek(0);
            reader.readString();
        }
    }

    /// <summary>
    /// | Method    | Mean     | Error     | StdDev    | Median   | Allocated |
    /// |---------- |---------:|----------:|----------:|---------:|----------:|
    /// | MethodOld | 8.735 us | 0.2605 us | 0.7348 us | 8.650 us |     808 B |
    /// | MethodNew | 3.872 us | 0.2734 us | 0.7622 us | 3.600 us |    1096 B |
    /// </summary>
    [MemoryDiagnoser]
    public class RebroadcastMovementBenchmark
    {
        ByteBufOutPacket inPacketSource = new ByteBufOutPacket();

        ByteBufInPacket inPacket;
        [GlobalSetup]
        public void Setup()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            inPacketSource.writeByte(1);
            inPacketSource.writeShort(32000);
            inPacketSource.writeString("qwertyuiopasdfghjklzxcvbnm123456789");
            inPacketSource.writeLong(1234567890123456789L);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            inPacket = new ByteBufInPacket(inPacketSource.getBytes());
        }

        [Benchmark]
        public void MethodOld()
        {
            var outPacket = new ByteBufOutPacket();
            inPacket.seek(3);
            PacketCommon.rebroadcastMovementListOld(outPacket, inPacket, 20);
        }

        [Benchmark]
        public void MethodNew()
        {
            var outPacket = new ByteBufOutPacket();
            inPacket.seek(3);
            PacketCommon.RebroadcastMovementList(outPacket, inPacket, 20);
        }
    }
}
