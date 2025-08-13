using Application.Core.Channel.ResourceTransaction.Handlers;

namespace Application.Core.Channel.ResourceTransaction
{
    public class ResourceTransactionManager
    {
        private readonly List<IResourceHandler> _handlers = new();
        private CancellationTokenSource tokenSource = new();
        public void AddResourceHandler(IResourceHandler handler)
        {
            _handlers.Add(handler);
        }

        // 事务性执行，所有资源一起扣除成功或失败回滚
        public bool ExecuteTransaction(Func<CancellationToken, bool> businessLogic)
        {
            // 1. 冻结所有资源
            foreach (var handler in _handlers)
            {
                if (!handler.TryFreeze(/*对应数量*/))
                {
                    // 冻结失败，回滚已冻结资源
                    foreach (var h in _handlers)
                        h.Rollback();
                    return false;
                }
            }

            bool success = false;
            try
            {
                // 2. 执行业务逻辑
                success = businessLogic(tokenSource.Token);
            }
            catch
            {
                return false;
            }
            finally
            {
                // 3. 根据结果提交或回滚
                foreach (var handler in _handlers)
                {
                    if (success)
                        handler.Commit();
                    else
                        handler.Rollback();
                }
            }
            return success;
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }
    }
}
