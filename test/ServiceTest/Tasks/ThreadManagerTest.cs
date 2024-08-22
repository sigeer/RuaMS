using server;

namespace ServiceTest.Tasks
{
    public class ThreadManagerTest
    {
        [Test]
        public void NewTask_Test()
        {
            ThreadManager.getInstance().newTask(() =>
            {
                Console.WriteLine("new task");
                throw new Exception();
            });
            Assert.Pass();
        }
    }
}
