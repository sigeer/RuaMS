using Application.Utility.Performance;
using Serilog;
using System.Diagnostics;
using System.Threading.Channels;

namespace Application.Utility.Pipeline
{
    public abstract class CommandLoop<TContext> : IAsyncDisposable where TContext: class, ICommandContext
    {
        private readonly Channel<ICommand<TContext>> _commands;
        private Task? _runningTask;

        public CommandLoop()
        {
            _commands = Channel.CreateBounded<ICommand<TContext>>(new BoundedChannelOptions(100_000)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
        }

        public void Start(string key)
        {
            if (_runningTask != null)
                throw new InvalidOperationException("Already running");

            _runningTask = RunLoopAsync(key);
            return;
        }

        protected abstract TContext CreateContext();

        private async Task RunLoopAsync(string key)
        {
            Stopwatch sw = new();

            while (await _commands.Reader.WaitToReadAsync())
            {
                GameMetrics.CommandCountTick(key, _commands.Reader.Count);
                while (_commands.Reader.TryRead(out var item))
                {
                    sw.Restart();
                    item.Execute(CreateContext());
                    sw.Stop();
                    GameMetrics.GameTick(key, sw.Elapsed.TotalMilliseconds);
                }
            }

            //await foreach (var item in _commands.Reader.ReadAllAsync())
            //{
            //    sw.Restart();
            //    await item.Execute(CreateContext());
            //    sw.Stop();
            //    GameMetrics.GameTick(key, sw.Elapsed.TotalMilliseconds);
            //}
        }

        public void Register(ICommand<TContext> tickable)
        {
            if (!_commands.Writer.TryWrite(tickable))
            {
                Log.Logger.Fatal("命令写入失败 {Info}，当前命令队列长度 {Length}", tickable.GetType().Name, _commands.Reader.Count);
            }
        }

        public async Task StopAsync()
        {
            _commands.Writer.TryComplete();

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
