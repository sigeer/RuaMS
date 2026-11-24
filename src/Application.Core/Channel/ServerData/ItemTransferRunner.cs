using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.ServerData
{
    /// <summary>
    /// 物品转移的统一调度：占位 → 本地背包 → 提交，失败就回滚。
    /// <para>
    /// 领取、上架、拍卖上架、求购成交四处共用这一份流程，各自只提供自己的五个步骤。
    /// 之前每处都各写一遍“先扣/先加、远程失败怎么办”，结果有的地方会复制道具、
    /// 有的地方会丢件 —— 现在这些差异全部收在这里。
    /// </para>
    /// <para>
    /// 之所以能在失败后重试而不出错，是因为中心服按 token 幂等（见
    /// <c>ItemTransferJournal</c>）：同一个 token 的 <c>COMMIT</c> 无论发多少次，
    /// 都只生效一次、并返回同一个结论。于是“结果未知”可以靠重试消成“确定”。
    /// </para>
    /// </summary>
    public sealed class ItemTransferRunner
    {
        const int MaxAttempts = 3;

        readonly Microsoft.Extensions.Logging.ILogger _logger;

        public ItemTransferRunner(Microsoft.Extensions.Logging.ILogger logger)
        {
            _logger = logger;
        }

        public async Task<TransferOutcome> RunAsync(ItemTransfer transfer)
        {
            var token = Yitter.IdGenerator.YitIdHelper.NextId();

            if (!await TryPrepare(transfer, token))
            {
                return TransferOutcome.Rejected;
            }

            bool applied;
            try
            {
                applied = await transfer.ApplyLocal();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "物品转移本地背包操作异常（token={Token}），撤销占位", token);
                applied = false;
            }

            if (!applied)
            {
                await SafeAbort(transfer, token);
                return TransferOutcome.LocalFailed;
            }

            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    if (await transfer.Commit(token))
                    {
                        return TransferOutcome.Done;
                    }
                }
                catch (Exception ex)
                {
                    if (attempt < MaxAttempts)
                    {
                        _logger.LogWarning(ex, "物品转移提交失败，第 {Attempt} 次重试（token={Token}）", attempt, token);
                        await Task.Delay(200 * attempt);
                        continue;
                    }

                    // 保留本地结果：删掉一份可能其实已经提交成功的物品才是真丢东西。
                    // 中心服那一侧同样留着（占位过期后物品在玩家待领取列表里），可人工对账。
                    _logger.LogError(ex, "物品转移提交结果未知（token={Token}），已保留本地结果", token);
                    return TransferOutcome.Unknown;
                }

                // 中心服明确拒绝：回滚本地、撤销占位
                await SafeRollback(transfer, token);
                await SafeAbort(transfer, token);
                return TransferOutcome.Rejected;
            }
        }

        async Task<bool> TryPrepare(ItemTransfer transfer, long token)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await transfer.Prepare(token);
                }
                catch (Exception ex)
                {
                    if (attempt >= MaxAttempts)
                    {
                        // 本地还没动过，直接当作被拒绝
                        _logger.LogError(ex, "物品转移占位失败（token={Token}）", token);
                        return false;
                    }

                    _logger.LogWarning(ex, "物品转移占位失败，第 {Attempt} 次重试（token={Token}）", attempt, token);
                    await Task.Delay(200 * attempt);
                }
            }
        }

        async Task SafeAbort(ItemTransfer transfer, long token)
        {
            try
            {
                await transfer.Abort(token);
            }
            catch (Exception ex)
            {
                // 撤销失败不影响玩家：中心服的占位会自然过期
                _logger.LogWarning(ex, "物品转移撤销占位失败（token={Token}）", token);
            }
        }

        async Task SafeRollback(ItemTransfer transfer, long token)
        {
            try
            {
                await transfer.RollbackLocal();
            }
            catch (Exception ex)
            {
                // 回滚失败会导致玩家背包里多一份 —— 必须留痕
                _logger.LogError(ex, "物品转移回滚本地失败，玩家背包可能多出物品（token={Token}）", token);
            }
        }
    }
}
