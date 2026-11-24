namespace Application.Core.Channel.ServerData
{
    /// <summary>
    /// 一次物品转移的五步操作。由具体业务填充，交给 <see cref="ItemTransferRunner"/> 统一调度。
    /// <para>
    /// 除 <see cref="Commit"/> 之外的每一步都要求“失败就是失败”：
    /// 返回 <c>false</c> 表示中心服明确拒绝，调用方据此回滚。
    /// </para>
    /// </summary>
    public sealed class ItemTransfer
    {
        /// <summary>
        /// 占位：中心服校验并暂存，返回 <c>false</c> 表示被拒绝（此时本地什么都还没动）。
        /// 这个阶段可以安全重试。
        /// </summary>
        public required Func<long, Task<bool>> Prepare { get; init; }

        /// <summary>
        /// 本地背包操作（进背包 / 扣背包）。返回 <c>false</c> 时runner 会自动撤销占位。
        /// </summary>
        public required Func<Task<bool>> ApplyLocal { get; init; }

        /// <summary>
        /// 落定。返回 <c>false</c> 表示中心服明确拒绝，runner 会回滚本地并撤销占位；
        /// 抛异常表示结果未知，runner 会用同一个 token 重试（中心服幂等，重试一定能问出结论）。
        /// </summary>
        public required Func<long, Task<bool>> Commit { get; init; }

        /// <summary>
        /// 撤销占位。只应在确定没有提交成功时调用。
        /// </summary>
        public required Func<long, Task> Abort { get; init; }

        /// <summary>
        /// 回滚本地背包操作。
        /// </summary>
        public required Func<Task> RollbackLocal { get; init; }
    }
}
