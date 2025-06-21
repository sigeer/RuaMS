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
            Server.getInstance().Reset();
            await Server.getInstance().InitializeTimelyTasks(TaskEngine.Task);
            await Server.getInstance().GlobalTimerManager.Stop();
        }

        [Benchmark()]
        public async Task UseQuartz()
        {
            Server.getInstance().Reset();
            await Server.getInstance().InitializeTimelyTasks(TaskEngine.Quartz);
            await Server.getInstance().GlobalTimerManager.Stop();
        }
    }
}
