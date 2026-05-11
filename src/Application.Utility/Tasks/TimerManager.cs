using DotNetty.Common.Utilities;

namespace Application.Utility.Tasks
{
    public class TimerManager
    {
        readonly HashedWheelTimer _timer;

        public TimerManager()
        {
            _timer = new HashedWheelTimer();
        }
        public ScheduledFuture Schedule(string group, string name, Action r, TimeSpan delay)
        {
            return new TimerTaskScheduledFuture(group, name, _timer.NewTimeout(new TimerTask(r), delay));
        }
        public ScheduledFuture Register(string group, string name, Action r, TimeSpan period, TimeSpan delay)
        {
            return new TimerScheduledFuture(group, name, new Timer(_ => r(), null, delay, period));
        }

        public ScheduledFuture Schedule(string group, string name, Func<Task> r, TimeSpan delay)
        {
            return new TimerTaskScheduledFuture(group, name, _timer.NewTimeout(new TimerTask(r), delay));
        }
        public ScheduledFuture Register(string group, string name, Func<Task> r, TimeSpan period, TimeSpan delay)
        {
            return new TimerScheduledFuture(group, name, new Timer(async _ => await r(), null, delay, period));
        }
    }
}
