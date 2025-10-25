using System.Collections.Concurrent;
using System.Diagnostics.Tracing;

namespace Application.Utility
{
    [EventSource(Name = "MethodTracker")]
    public class MethodEventSource : EventSource
    {
        public static readonly MethodEventSource Instance = new MethodEventSource();

        private ConcurrentDictionary<string, EventCounter> _counters = new();

        private MethodEventSource() { }

        /// <summary>
        /// 记录方法调用次数
        /// </summary>
        public void TrackCall(string methodName)
        {
            // 获取或创建 EventCounter
            var counter = _counters.GetOrAdd(methodName, name =>
            {
                var c = new EventCounter(name, this)
                {
                    DisplayName = name
                };
                return c;
            });

            counter.WriteMetric(1); // 每调用一次方法计数 +1
        }
    }

}
