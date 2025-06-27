using Application.Utility.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using net.server;
using server;

namespace Application.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 10)]
    public class TaskBenchmark
    {

        [Benchmark()]
        public async Task UseTask()
        {
            var timeManager = await TimerManager.InitializeAsync(TaskEngine.Task, "Benchmark");
            await timeManager.Stop();
        }

        [Benchmark()]
        public async Task UseQuartz()
        {
            var timeManager = await TimerManager.InitializeAsync(TaskEngine.Quartz, "Benchmark");
            await timeManager.Stop();
        }
    }
}
