namespace Application.Core.Compatible.Extensions
{
    // 不推荐使用， 最后锁搞不定了用这个
    public static class MonitorEx
    {
        public static void TryExit(object? obj)
        {
            if (obj == null)
                return;

            if (Monitor.IsEntered(obj))
                Monitor.Exit(obj);
        }
    }

    public static class ReaderWriteLockSlimExtensions
    {
        public static void TryExitReadLock(ReaderWriterLockSlim lockObj)
        {
            if (lockObj.IsReadLockHeld)
                lockObj.ExitReadLock();
        }

        public static void TryExitWriteLock(ReaderWriterLockSlim lockObj)
        {
            if (lockObj.IsWriteLockHeld)
                lockObj.ExitWriteLock();
        }
    }

}
