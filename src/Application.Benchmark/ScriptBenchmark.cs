using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using ServiceTest.Infrastructure.Scripts;

namespace Application.Benchmark
{
    [RankColumn]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 100)]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
    public class ScriptBenchmark
    {

        [Params(ScriptType.Jint, ScriptType.NLua)]
        public ScriptType Type { get; set; }


        [Benchmark()]
        public void ReturnScriptNewObject()
        {
            BaseScriptTest p;
            if (Type == ScriptType.NLua)
                p = new NLuaScriptEngineTest();
            else
                p = new JintScriptEngineTest();
            p.ReturnScriptNewObject();
        }

        [Benchmark()]
        public void ReturnScriptNewObjectArray()
        {
            BaseScriptTest p;
            if (Type == ScriptType.NLua)
                p = new NLuaScriptEngineTest();
            else
                p = new JintScriptEngineTest();
            p.ReturnScriptNewObjectArray();
        }


        [Benchmark()]
        public void UpdateObject()
        {
            BaseScriptTest p;
            if (Type == ScriptType.NLua)
                p = new NLuaScriptEngineTest();
            else
                p = new JintScriptEngineTest();
            p.UpdateObject();
        }

    }

    public enum ScriptType
    {
        Jint,
        NLua
    }
}
