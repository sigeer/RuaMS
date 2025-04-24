using Application.Core;
using Application.Core.Game;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using net.server;
using Serilog;
using Serilog.Events;
using System.Text;

namespace Application.Benchmark
{
    [RankColumn]
    [MemoryDiagnoser()]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 5)]
    public class ScriptBenchmark
    {
        void Initialize()
        {
            // Environment.SetEnvironmentVariable("ms-wz", "D:\\Cosmic\\wz");
            // 支持GBK
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            GlobalTools.Encoding = Encoding.GetEncoding("GBK");
        }


        [Benchmark()]
        public async Task UseJint()
        {
            Initialize();
            ScriptDir.Event = "event";
            await Server.getInstance().Start();
        }

        [Benchmark()]
        public async Task UseNLua()
        {
            Initialize();
            ScriptDir.Event = "event-lua";
            await Server.getInstance().Start();
        }
    }
}
