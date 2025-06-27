using net.server;
using server;

namespace ServiceTest.Infrastructure
{
    internal class TimerManagerTests : TestBase
    {
        [Test]
        public async Task RegisterTaskTests()
        {
            var timeManager = await TimerManager.InitializeAsync(Application.Utility.Tasks.TaskEngine.Quartz, "test");
            int count = 0;
            var task = timeManager.register(() =>
            {
                count++;
            }, TimeSpan.FromSeconds(1));
            await Task.Delay(TimeSpan.FromSeconds(3));
            await task.CancelAsync(true);
            await Task.Delay(TimeSpan.FromSeconds(3));
            Assert.That(count, Is.EqualTo(4));
        }
    }
}
