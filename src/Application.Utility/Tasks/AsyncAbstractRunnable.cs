using Application.Utility.Loggers;
using Serilog;

namespace Application.Utility.Tasks
{
    public abstract class AsyncAbstractRunnable
    {
        protected ILogger log;
        protected AsyncAbstractRunnable()
        {
            Name = GetType().Name;
            log = LogFactory.GetLogger($"AbstractRunnable/{Name}");
        }

        protected AsyncAbstractRunnable(string name)
        {
            Name = name;
            log = LogFactory.GetLogger($"AbstractRunnable/{Name}");
        }

        public abstract Task RunAsync();

        public string Name { get; set; }
    }
}
