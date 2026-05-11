using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Utility.Tasks
{
    public class TimerTaskScheduledFuture : ScheduledFuture
    {
        public string Group { get; }
        public string Name { get; }
        readonly ITimeout _job;

        public TimerTaskScheduledFuture(string group, string name, ITimeout job)
        {
            Group = group;
            Name = name;
            _job = job;
        }

        public string JobId => _job.ToString();

        public bool cancel(bool immediately)
        {
            return _job.Cancel();
        }

        public Task<bool> CancelAsync(bool immediately)
        {
            _job.Cancel();
        }
    }

    public class TimerScheduledFuture : ScheduledFuture
    {
        public string Group { get; }
        public string Name { get; }
        readonly Timer _job;

        public TimerScheduledFuture(string group, string name, Timer job)
        {
            Group = group;
            Name = name;
            _job = job;
        }

        public string JobId => _job.ToString();

        public bool cancel(bool immediately)
        {
            _job.Dispose();
        }

        public Task<bool> CancelAsync(bool immediately)
        {
            _job.Cancel();
        }
    }
}
