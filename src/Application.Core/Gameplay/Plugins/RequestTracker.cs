namespace Application.Core.Gameplay.Plugins
{

    /// <summary>
    /// 跟踪活跃请求，支持优雅关闭（排水）
    /// </summary>
    public sealed class RequestTracker : IAsyncDisposable
    {
        private int _activeCount = 0;
        private readonly TaskCompletionSource _completionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _disposeRequested = false;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 进入请求，返回一个 disposable，离开作用域时自动减少计数。
        /// 若已请求卸载，则抛出 InvalidOperationException。
        /// </summary>
        public IDisposable EnterRequest()
        {
            if (Volatile.Read(ref _disposeRequested))
                throw new InvalidOperationException("Plugin is being disposed, new requests are rejected.");

            Interlocked.Increment(ref _activeCount);
            return new DisposableAction(Dismiss);
        }

        private void Dismiss()
        {
            int newCount = Interlocked.Decrement(ref _activeCount);
            if (newCount == 0 && Volatile.Read(ref _disposeRequested))
            {
                _completionSource.TrySetResult();
            }
        }

        /// <summary>
        /// 等待所有活跃请求完成（排水），支持超时。
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(_defaultTimeout);
        }

        public async ValueTask DisposeAsync(TimeSpan timeout)
        {
            // 标记不再接受新请求
            Volatile.Write(ref _disposeRequested, true);

            // 如果当前没有活跃请求，直接返回
            if (Volatile.Read(ref _activeCount) == 0)
                return;

            // 等待所有请求完成，带超时
            var completedTask = _completionSource.Task;
            var timeoutTask = Task.Delay(timeout);
            var winner = await Task.WhenAny(completedTask, timeoutTask).ConfigureAwait(false);

            if (winner == timeoutTask)
            {
                // 超时：强制完成（可能导致资源泄漏，但避免永久阻塞）
                _completionSource.TrySetResult();
            }

            // 确保完成（避免在超时后立即返回导致竞态）
            await completedTask.ConfigureAwait(false);
        }

        private sealed class DisposableAction : IDisposable
        {
            private Action _action;
            public DisposableAction(Action action) => _action = action;
            public void Dispose() => Interlocked.Exchange(ref _action, null)?.Invoke();
        }
    }
}
