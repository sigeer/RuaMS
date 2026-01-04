using Application.Utility.Loggers;
using Serilog;
using System.Threading.Tasks;

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

    public class FuncAsyncRunnable : AsyncAbstractRunnable
    {
        private Func<Task>? _action;
        public FuncAsyncRunnable(string name, Func<Task> action) : base(name)
        {
            _action = action;
        }

        public override async Task RunAsync()
        {
            if (_action != null)
            {
                await _action.Invoke();
            }
        }
    }
}
