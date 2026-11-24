namespace Application.Core.Login.ServerData
{
    /// <summary>
    /// 物品转移的占位日志 —— 一套机制同时服务「领取 / 上架 / 拍卖上架 / 求购成交」。
    /// <para>
    /// 这四件事本质相同：一份物品在「频道背包」和「中心服仓库」之间搬家，而跨进程调用
    /// 可能超时、可能断连，结果对频道侧来说是未知的。收敛办法是把一次操作拆成
    /// <c>PREPARE → (本地背包操作) → COMMIT</c>，并用一个 channel 侧生成的 token 贯穿全程：
    /// </para>
    /// <list type="bullet">
    /// <item>同 token 重放 <c>PREPARE</c> / <c>COMMIT</c> 返回同一个结论，绝不重复执行 ——
    /// 于是“未知结果”可以靠重试消掉，不需要各操作自己写探测代码；</item>
    /// <item>同一件商品同时只允许一个 token 在途，并发领取/并发成交的第二笔直接拿到
    /// <see cref="Application.Shared.Items.MTSResponseCode.InProgress"/>；</item>
    /// <item>频道进程中途死掉时没人发 <c>ABORT</c>，条目自然过期；真正的物品兜底不靠这里 ——
    /// <c>MTSManager.ParkItem</c> 在 <c>PREPARE</c> 时就把物品落成一条“待领取”记录，
    /// 频道没来 <c>COMMIT</c> 的话它就一直留在玩家自己的待领取列表里，玩家能领回去。</item>
    /// </list>
    /// <para>
    /// 条目只放内存：它只是“一次请求的中间状态”，进程重启后自然清空。重启前没走完的操作，
    /// 物品已经躺在玩家的待领取列表里，重来一次即可。
    /// </para>
    /// </summary>
    public sealed class ItemTransferJournal
    {
        /// <summary>
        /// 一次在途的物品转移
        /// </summary>
        public sealed class Entry
        {
            public long Token { get; init; }
            /// <summary>
            /// 发起转移的角色。别的角色拿同一个 token 来收尾一律拒绝。
            /// </summary>
            public int OwnerId { get; init; }
            /// <summary>
            /// 本次转移的“标的”：领取时是待领取的那一行，上架时是新暂存的行，
            /// 求购成交时是那张求购单。
            /// </summary>
            public int ProductId { get; init; }
            /// <summary>
            /// 暂存出来的交付行（上架 / 求购成交用），<c>0</c> 表示没有。
            /// </summary>
            public int ParkedId { get; init; }
            public ProtoModel.TransferStage Stage { get; init; }
            public long ExpiresAt { get; init; }

            public Entry Clone() => new()
            {
                Token = Token,
                OwnerId = OwnerId,
                ProductId = ProductId,
                ParkedId = ParkedId,
                Stage = Stage,
                ExpiresAt = ExpiresAt,
            };
        }

        /// <summary>
        /// 条目存活时长。正常流程几毫秒就走完，留 5 分钟纯粹是给“频道卡住但没死”兜底；
        /// 过期不影响物品安全（物品在待领取列表里），只是允许该商品被重新占位。
        /// </summary>
        const int EntryTtlMs = 5 * 60 * 1000;

        const int SweepIntervalMs = 60_000;

        readonly object _gate = new();
        readonly Dictionary<long, Entry> _entries = [];
        long _nextSweepAt;

        /// <summary>
        /// 开始一次转移。重复的 token 返回同一份（幂等）；同一件商品已有别的 token 在途、
        /// 或 token 被别人占用时返回 <c>null</c>。
        /// </summary>
        public Entry? Prepare(long token, int ownerId, int productId, long nowMs)
        {
            lock (_gate)
            {
                SweepIfDue(nowMs);

                if (_entries.TryGetValue(token, out var existing))
                {
                    return existing.OwnerId == ownerId ? existing.Clone() : null;
                }

                if (productId > 0 && _entries.Values.Any(x => x.ProductId == productId && x.ExpiresAt > nowMs))
                {
                    return null;
                }

                var entry = new Entry
                {
                    Token = token,
                    OwnerId = ownerId,
                    ProductId = productId,
                    Stage = ProtoModel.TransferStage.Prepare,
                    ExpiresAt = nowMs + EntryTtlMs,
                };
                _entries[token] = entry;
                return entry.Clone();
            }
        }

        /// <summary>
        /// 回填暂存出来的交付行（上架 / 求购成交在 <c>PREPARE</c> 里建完行之后调用）
        /// </summary>
        public bool AttachParked(long token, int ownerId, int parkedId)
        {
            lock (_gate)
            {
                if (!_entries.TryGetValue(token, out var entry) || entry.OwnerId != ownerId)
                {
                    return false;
                }

                var next = new Entry
                {
                    Token = entry.Token,
                    OwnerId = entry.OwnerId,
                    ProductId = entry.ProductId,
                    ParkedId = parkedId,
                    Stage = entry.Stage,
                    ExpiresAt = entry.ExpiresAt,
                };
                _entries[token] = next;
                return true;
            }
        }

        /// <summary>
        /// 推进阶段。<c>COMMIT</c> 是终态，之后再来的 <c>ABORT</c> 不会把它退回去。
        /// </summary>
        public void Reach(long token, int ownerId, ProtoModel.TransferStage stage)
        {
            lock (_gate)
            {
                if (!_entries.TryGetValue(token, out var entry) || entry.OwnerId != ownerId)
                {
                    return;
                }

                if (entry.Stage == ProtoModel.TransferStage.Commit && stage == ProtoModel.TransferStage.Abort)
                {
                    // 已经落定了，放弃不生效 —— 否则会变成“商品没了但物品退回来了”
                    return;
                }

                _entries[token] = new Entry
                {
                    Token = entry.Token,
                    OwnerId = entry.OwnerId,
                    ProductId = entry.ProductId,
                    ParkedId = entry.ParkedId,
                    Stage = stage,
                    ExpiresAt = entry.ExpiresAt,
                };
            }
        }

        public Entry? Get(long token, int ownerId)
        {
            lock (_gate)
            {
                return _entries.TryGetValue(token, out var entry) && entry.OwnerId == ownerId ? entry.Clone() : null;
            }
        }

        /// <summary>
        /// 只删自己的条目。别人的 token 一律不动。
        /// </summary>
        public void Drop(long token, int ownerId)
        {
            lock (_gate)
            {
                if (_entries.TryGetValue(token, out var entry) && entry.OwnerId == ownerId)
                {
                    _entries.Remove(token);
                }
            }
        }

        void SweepIfDue(long nowMs)
        {
            if (nowMs < _nextSweepAt)
            {
                return;
            }

            _nextSweepAt = nowMs + SweepIntervalMs;

            foreach (var stale in _entries.Values.Where(x => x.ExpiresAt <= nowMs).ToList())
            {
                _entries.Remove(stale.Token);
            }
        }
    }
}
