using DotNetty.Common.Utilities;

namespace Application.Utility.Tasks
{
    public class DelegatingTimerTask : ITimerTask
    {
        readonly Action data;

        public DelegatingTimerTask(Action data)
        {
            this.data = data;
        }

        public void Run(ITimeout timeout)
        {
            data();
        }
    }
}
