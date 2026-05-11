using DotNetty.Common.Utilities;

namespace Application.Utility.Tasks
{
    public class TimerTask : ITimerTask
    {
        readonly object data;

        public TimerTask(object data)
        {
            this.data = data;
        }

        public void Run(ITimeout timeout)
        {
            if (data is Action a)
            {
                a();
            }
            else if (data is Func<Task> f)
            {
                _ = f();
            }
            throw new NotImplementedException();
        }
    }
}
