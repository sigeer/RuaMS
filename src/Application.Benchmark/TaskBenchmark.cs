using Application.Utility.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Application.Benchmark
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, iterationCount: 10)]
    public class TaskBenchmark
    {

        [Benchmark()]
        public async Task UseTask()
        {
            var timeManager = await TimerManagerFactory.InitializeAsync(TaskEngine.Task, "Benchmark");
            await timeManager.Stop();
        }

        [Benchmark()]
        public async Task UseQuartz()
        {
            var timeManager = await TimerManagerFactory.InitializeAsync(TaskEngine.Quartz, "Benchmark");
            await timeManager.Stop();
        }
    }
}
