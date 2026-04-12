using Application.Utility.Performance;
using Serilog;
using System.Diagnostics;
using System.Threading.Channels;

namespace Application.Utility.Pipeline
{
    public interface ICommandPipeline
    {
        void Start();
    }
    public class CommandLoop<TContext> : IAsyncDisposable, ICommandPipeline where TContext : IActorInstance<TContext>
    {
        private TContext _context;
        private Task? _runningTask;
        private Channel<ICommand> _command;

        public CommandLoop(TContext context)
        {
            _context = context;
            _command = Channel.CreateBounded<ICommand>(new BoundedChannelOptions(100_000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
        }

        public void Start()
        {
            if (_runningTask != null)
                throw new InvalidOperationException("Already running");

            _runningTask = RunLoopAsync();
            return;
        }

        private async Task RunLoopAsync()
        {
            Stopwatch sw = Stopwatch.StartNew();
            await foreach (var item in _command.Reader.ReadAllAsync())
            {
                Activity? activity = null;
                if (item is not IIgnoreActivityCommand)
                {
                    activity = GameMetrics.ActivitySource.StartActivity("ActorActivity");
                    activity?.SetTag("ActorType", typeof(TContext).Name);
                    activity?.SetTag("Instance", _context.InstanceName);
                    activity?.SetTag("Command", item.Name);
                }

                try
                {
                    sw.Restart();

                    if (item is IAsyncCommand<TContext> a)
                        await a.Execute(_context);
                    else if (item is ICommand<TContext> b)
                        b.Execute(_context);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex.ToString());
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.AddEvent(new ActivityEvent("exception",
                        tags: new ActivityTagsCollection
                        {
                            ["exception.type"] = ex.GetType().Name,
                            ["exception.message"] = ex.Message,
                            ["exception.stacktrace"] = ex.StackTrace ?? ""
                        }));
                }
                finally
                {
                    sw.Stop();
                    GameMetrics.GameTick(_context.InstanceName, sw.Elapsed.TotalMilliseconds);
                    activity?.Dispose();
                    activity = null;
                }
            }
        }

        public void Register(ICommand tickable)
        {
            if (!_command.Writer.TryWrite(tickable))
            {
                Log.Logger.Fatal("命令写入失败 {Info}，当前命令队列长度 {Length}", tickable.GetType().Name, _command.Reader.Count);
            }
        }

        public async Task StopAsync()
        {
            _command.Writer.TryComplete();

            if (_runningTask != null)
            {
                await _runningTask;
            }
            _runningTask = null;
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}
