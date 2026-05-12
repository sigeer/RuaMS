using DotNetty.Common.Utilities;

namespace Application.Utility.Tasks
{
    public class TimerTaskScheduledFuture : ScheduledFuture
    {
        readonly ITimeout _job;

        public TimerTaskScheduledFuture(JobKey jobKey, ITimeout job)
        {
            JobId = jobKey;
            _job = job;
        }

        public JobKey JobId { get; }

        public void cancel()
        {
            _job.Cancel();
        }

        public void Dispose()
        {
            _job.Cancel();
        }
    }

    public class TimerScheduledFuture : ScheduledFuture
    {

        private CancellationTokenSource _syncCts;
        private bool disposedValue;

        public CancellationToken Token => _syncCts.Token;

        public TimerScheduledFuture(JobKey jobKey)
        {
            JobId = jobKey;
            _syncCts = new();
        }

        public JobKey JobId { get; }

        public void cancel()
        {
            _syncCts.Cancel();
        }

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    cancel();
                    _syncCts.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~TimerScheduledFuture()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
