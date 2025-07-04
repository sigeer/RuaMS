namespace Application.Shared.Items
{
    public enum ItemTransactionStatus
    {
        /// <summary>
        /// 未处理（没有执行到这里）时
        /// </summary>
        Pending,
        /// <summary>
        /// 等待执行Commit
        /// </summary>
        PendingForCommit,
        /// <summary>
        /// 等待执行Rollback
        /// </summary>
        PendingForRollback,
        /// <summary>
        /// 完成（用不上，完成后会被移除）
        /// </summary>
        Completed

    }
}
