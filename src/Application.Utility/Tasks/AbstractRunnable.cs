using Application.Utility.Loggers;
using Serilog;
using System.Diagnostics;

namespace Application.Utility.Tasks
{
    /// <summary>
    /// 具名任务
    /// </summary>
    public abstract class AbstractRunnable
    {
        protected ILogger log;
        protected AbstractRunnable()
        {
            Name = GetType().Name;
            log = LogFactory.GetLogger($"AbstractRunnable/{Name}");
        }

        protected AbstractRunnable(string name)
        {
            Name = name;
            log = LogFactory.GetLogger($"AbstractRunnable/{Name}");
        }

        public virtual void HandleRun()
        {
            return;
        }

        public void run()
        {
            HandleRun();
        }

        public string Name { get; set; }
    }

    public class EmptyRunnable : AbstractRunnable
    {
    }

    public class NamedRunnable: AbstractRunnable
    {
        private Action _action;
        public NamedRunnable(string name, Action action) : base(name)
        {
            _action = action;
        }

        public override void HandleRun()
        {
            _action?.Invoke();
        }
    }

    public class TempRunnable : NamedRunnable
    {
        private static string[] Special = [nameof(ITimerManager.register), nameof(ITimerManager.schedule), nameof(ITimerManager.scheduleAtTimestamp)];

        public TempRunnable(string name, Action action) : base(name, action)
        {
        }

        public static TempRunnable Parse(Action action)
        {
            string taskName = "Temp_" + Guid.NewGuid().ToString();

            var st = new StackTrace();
            var stIndex = 1;
            var refer = st.GetFrame(stIndex);
            while (refer != null)
            {
                var caller = refer.GetMethod();
                if (caller != null)
                {
                    var methodName = caller.Name;
                    if (Special.Contains(methodName))
                    {
                        refer = st.GetFrame(++stIndex);
                    }
                    else
                    {
                        var typeName = caller?.DeclaringType?.Name ?? "UnknownClass";
                        taskName = $"{typeName}_{methodName}_{Guid.NewGuid().ToString()}";
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            return new TempRunnable(taskName, action);
        }
    }
}
