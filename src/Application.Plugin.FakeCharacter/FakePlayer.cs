using Application.Core.constants;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Shared.Constants;
using Application.Shared.Net;
using server.movement;
using SyncProto;
using System.Drawing;
using tools;

namespace Application.Plugin.FakeCharacter
{
    /// <summary>
    /// 假人玩家 —— 伪造不存在客户端的玩家，以广播数据包模拟其行为。
    ///
    /// ============================================================
    ///  移动系统原理（IDA 反汇编 CMovePath::Decode @ 0x68a33c 确认）
    /// ============================================================
    ///
    /// MOVE_PLAYER 广播包结构：
    ///   [chrId:int][startPos:Point(short X, short Y)][movement_list...]
    ///
    /// movement_list 由 CMovePath::Encode 生成，结构为：
    ///   [numElements:byte]  // 移动指令条数（<=0 时空包，客户端忽略）
    ///   [elem_0...]         // 每条 8~16 字节取决于 command
    ///   [padding:byte]      // 尾部填充
    ///   [nibble_keys...]    // 按键状态编码（每字节编 2 个 nibble）
    ///   [x_velocity:i2][y_velocity:i2][unk1:i2][unk2:i2]  // 尾声段
    ///
    /// === CMovePath::ELEM 结构体 (sizeof=0x18=24B, memset 清零) ===
    ///   偏移    大小    字段           汇编指令（mov [esi+OFF], ax）
    ///   ------  ------  -------------- -------------------------------------
    ///   0x00    1B      command        68a3ad: mov [esi], al
    ///   0x02    2B      x              68a3c7: mov [esi+2], ax
    ///   0x04    2B      y              68a3d2: mov [esi+4], ax
    ///   0x06    2B      vx/xwobble     68a3dd: mov [esi+6], ax
    ///   0x08    2B      vy/ywobble     68a3e8: mov [esi+8], ax
    ///   0x0A    1B      newstate       68a437: mov [esi+0Ah], al
    ///   0x0C    2B      fh（foothold）   68a3f1: mov [esi+0Ch], ax
    ///   0x0E    2B      fh_origin      68a4c6: mov [esi+0Eh], ax (仅 cmd=15)
    ///   0x10    2B      duration       68a43f: mov [esi+10h], ax
    ///   0x12    1B      wui            68a4d6: mov [esi+12h], al (仅 cmd=10)
    ///   0x13~0x17       [padding]
    ///
    /// === 各指令的通信线格式（IDA Decode 读取顺序）===
    ///
    ///   Cmd 0 / 5 / 17（AbsoluteLifeMovement，绝对移动）：
    ///     [cmd:1B][x:i2][y:i2][vx:i2][vy:i2][fh:i2][newstate:1B][duration:i2]
    ///     = 14 bytes
    ///     注释：服务端 AbsoluteLifeMovement.serialize() 与此一致 ✓
    ///
    ///   Cmd 15（JumpDownMovement）：
    ///     [cmd:1B][x:i2][y:i2][vx:i2][vy:i2][fh:i2][fh_origin:i2][newstate:1B][duration:i2]
    ///     = 16 bytes
    ///
    ///   Cmd 1 / 2 / 6 / 12 / 13 / 16 / 18 / 19 / 20 / 22（相对移动）：
    ///     [cmd:1B][xoff:i2][yoff:i2][newstate:1B][duration:i2]
    ///     = 8 bytes
    ///     注意：通信线中的 i2 写入 ELEM.vx/vy 而不是 x/y；
    ///           x/y 由 Decode 从上一个元素的 x/y 复制。
    ///           RelativeLifeMovement.serialize() 输出 (x,y) 但客户端
    ///           读到 (vx,vy) 中，最终视觉效果取决于客户端渲染逻辑。
    ///
    ///   Cmd 3 / 4 / 7 / 8 / 9 / 11（传送/技能突进）：
    ///     [cmd:1B][x:i2][y:i2][fh:i2][newstate:1B][duration:i2]
    ///     = 10 bytes
    ///
    ///     ⚠️ 服务端 TeleportMovement.serialize() 与此不兼容！
    ///        服务端输出：[cmd][x][y][vx][vy][newstate]（9 字节，无 fh/duration）
    ///        客户端期望：[cmd][x][y][fh][newstate][duration]（10 字节）
    ///        两者长度和字段都不同，会导致接收客户端对后续指令解析错位。
    ///
    ///   Cmd 10（ChangeEquip）：
    ///     [cmd:1B][wui:1B]
    ///     = 2 bytes
    ///     注意：cmd=10 跳过了公共尾部（无 newstate/duration），且 x/y 从
    ///           上一个元素复制，其他字段清零。
    ///
    ///   Cmd 14：
    ///     [cmd:1B][xoff:i2][yoff:i2][fh_origin:i2]
    ///     = 8 bytes（服务端在 parseMovement 中跳过 9 字节，略有差异）
    ///
    ///   Cmd 21（Aran 战斧）：
    ///     [cmd:1B][data:3B]

    /// === 姿态（newstate）编码 ===
    ///   bit0 = 方向（0=朝右, 1=朝左）
    ///   bit1+ = 动作类型（IDA 确认 MoveAction2RawAction @0x451ec8 如此拆分）
    ///   动作 ID 的视觉含义在客户端 WZ (Character.wz) 中定义，
    ///   以下值来自通用 MapleStory 开发经验，非 IDA 验证：
    ///   2/3 走  4/5. 站 6/7. 跳 16/17=1<<4. 爬绳子   朝右/左
    ///   （不成熟的猜测：6/7/8/9 可能对应攀爬/梯子相关，但 spawnPlayerMapObject
    ///     enteringField=true 硬编码 stance=6 为出生动画，说明 6 的实际含义并非攀爬）
    /// </summary>
    public class FakePlayer : Player
    {
        public FakePlayer(Player owner, IMap map, Point pos, int idx)
            : base(
                new FakeClient(),
                map,
                pos,
                new PlayerGetterDto
                {
                    Character = new Dto.CharacterDto
                    {
                        Sp = "0,0,0,0,0,0",
                        Hp = NumericConfig.MaxHP,
                        Maxhp = NumericConfig.MaxHP,
                        Mp = NumericConfig.MaxHP,
                        Maxmp = NumericConfig.MaxMP,
                    }
                }
            )
        {
            Id = GetFakePlayerId(owner, idx);
            Name = $"假人${idx}号";
            Face = 20000;
            Hair = 30030;
            Party = -1;

            // 同步主人的姿态。
            // stance 编码：bit0=方向(0右/1左), bit1+=动作类型（ID 含义在客户端 WZ 中定义）。
            // 简单做法：主人是什么姿态假人就设成什么姿态，客户端会渲染为相同动画。
            setStance(owner.getStance());
        }

        public override Player? Controller => this;
        public override int getObjectId()
        {
            return Id;
        }

        /// <summary>
        /// 假人没有真实客户端，不需要向自身发送数据包
        /// </summary>
        public override void sendPacket(Packet packet)
        {
            // 不存在客户端，不需要发送
        }

        public static int GetFakePlayerId(Player chr, int idx)
        {
            return 500000 + idx * 10000 + chr.Id;
        }

        protected override bool IsVisibleForPlayerWithoutRange(Player chr)
        {
            return true;
        }

        public override void OnTick(long now)
        {
            // TODO: 在此实现巡逻/跟随等定时行为
        }

        // ================================================================
        //  FH（Foothold）辅助方法
        // ================================================================

        /// <summary>
        /// 根据位置查找正确的 Foothold ID。
        ///
        /// fh = Foothold ID，是地图上地面线段的编号。客户端收到移动包后
        /// 用 fh 来确定角色站在哪个平台上，影响角色的 y 轴位置微调（z 排序）、
        /// 下落物理和交互判定。
        ///
        /// 虽然客户端主要靠 (x, y) 渲染位置，但错误的 fh 可能导致：
        ///   - 角色略微浮空或陷入地面
        ///   - 层次排序错误（角色显示在平台后面/前面）
        ///   - 在梯子/绳子附近交互异常
        ///
        /// 每个地图的 fh 数据在 WZ/Map.wz/MapHelper 中定义。
        /// MapModel.Footholds.FindBelowFoothold(pos) 会返回包含该点的
        /// 最上层脚趾线段；如果该点不在地面线段上（如空中），返回 null。
        /// </summary>
        /// <param name="pos">目标位置</param>
        /// <returns>fh ID，没有对应脚趾时返回 0（兜底）</returns>
        private int GetFoothold(Point pos)
        {
            return MapModel.Footholds.FindBelowFoothold(pos)?.getId() ?? 0;
        }

        // ================================================================
        //  行走移动 —— 使用 AbsoluteLifeMovement（Cmd 0）
        //  兼容性：服务端 AbsoluteLifeMovement.serialize()
        //         与客户端 CMovePath::Encode 的 case 0/5/17 完全一致 ✓
        // ================================================================

        /// <summary>
        /// 行走移动 —— 用 AbsoluteLifeMovement（Cmd 0）模拟正常行走。
        ///
        /// 通信格式：[cmd:1B][x:2B][y:2B][vx:2B][vy:2B][fh:2B][newstate:1B][duration:2B] = 14B
        /// IDA 确认：Decode @68a3be-68a3f1, Encode 对应字段
        ///
        /// fh 由目标位置自动查找（通过 MapModel.Footholds.FindBelowFoothold），
        /// 调用方无需关心——服务端 updatePosition 也直接跳过 fh，仅靠 (x, y) 更新位置。
        /// 通信线中包含 fh 是 CMovePath 结构的冗余设计，发送端预计算好供接收端缓存使用。
        ///
        /// 注意：如果起点到目标点距离很长（如跨屏幕），一条指令走长距离在客户端
        /// 看起来是"高速平移"而非正常行走。正确的做法是多次调用 WalkTo 步进，
        /// 或用 WalkToStepped / WalkPath 分段。
        /// </summary>
        /// <param name="targetPos">目标位置</param>
        /// <param name="durationMs">移动持续时间（毫秒），越小速度越快</param>
        public void WalkTo(Point targetPos, int durationMs = 400)
        {
            var startPos = getPosition();

            int dx = targetPos.X - startPos.X;
            int dy = targetPos.Y - startPos.Y;

            if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
            {
                BroadcastIdle();
                return;
            }

            // 姿态：2=朝右走，3=朝左走
            int newstate = dx >= 0 ? 2 : 3;

            // 速度向量 = 距离(像素) / 时间(秒) —— 客户端用它做帧间插值
            var velocity = new Point(
                (int)((long)Math.Abs(dx) * 1000 / Math.Max(durationMs, 1)),
                (int)((long)Math.Abs(dy) * 1000 / Math.Max(durationMs, 1))
            );

            List<LifeMovementFragment> movements =
            [
                new AbsoluteLifeMovement(0, targetPos, durationMs, newstate)
            ];
            ((AbsoluteLifeMovement)movements[0]).setPixelsPerSecond(velocity);
            ((AbsoluteLifeMovement)movements[0]).setFh(GetFoothold(targetPos));

            BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(targetPos);
        }

        /// <summary>
        /// 分段行走 —— 将长距离切分成多步，每段用 AbsoluteLifeMovement 连续输出。
        ///
        /// 这是正确的"走路"模拟方式：每段约 STEP_PX 像素，配合理的持续时间和速度。
        /// 默认步幅 120px, 每段 400ms → 速度约 300px/s，接近 MapleStory 角色
        /// 的基础行走速度。
        ///
        /// 每段的 fh 自动从终点坐标推导。
        /// </summary>
        /// <param name="targetPos">目标位置</param>
        /// <param name="stepPx">每段最大像素</param>
        /// <param name="segmentDurationMs">每段持续时间（毫秒）</param>
        public void WalkToStepped(Point targetPos, int stepPx = 120, int segmentDurationMs = 400)
        {
            var startPos = getPosition();
            int dx = targetPos.X - startPos.X;
            int dy = targetPos.Y - startPos.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= stepPx)
            {
                WalkTo(targetPos, segmentDurationMs);
                return;
            }

            // 计算段数，每段不超过 stepPx
            int steps = (int)Math.Ceiling(distance / stepPx);
            var path = new List<Point>(steps);
            double stepX = (double)dx / steps;
            double stepY = (double)dy / steps;

            for (int i = 1; i <= steps; i++)
            {
                path.Add(new Point(
                    startPos.X + (int)(stepX * i),
                    startPos.Y + (int)(stepY * i)
                ));
            }

            WalkPath(path, segmentDurationMs);
        }

        // ================================================================
        //  路径行走 —— 多段 AbsoluteLifeMovement 单包广播
        //  格式同上，每段独立设定位置/速度/姿态
        // ================================================================

        /// <summary>
        /// 沿路径行走 —— 把多个 AbsoluteLifeMovement 拼在一个包里一次性广播。
        /// 这是客户端 CMovePath 的典型用法，每条指令依次执行形成连贯行走。
        ///
        /// 服务端 AbsoluteLifeMovement.serialize() 的输出格式与
        /// 客户端 CMovePath::Encode 的 case 0 完全一致，没有兼容问题。
        ///
        /// 每段的 fh 自动从该段终点坐标推导。
        /// </summary>
        /// <param name="path">路径点列表（至少一个点）</param>
        /// <param name="segmentDurationMs">每段移动的毫秒数</param>
        /// <param name="finalStance">最后一段终点的姿态，null=自动根据方向决定</param>
        public void WalkPath(List<Point> path, int segmentDurationMs = 400, int? finalStance = null)
        {
            if (path.Count == 0) return;

            var startPos = getPosition();
            var movements = new List<LifeMovementFragment>();
            Point prevPos = startPos;

            for (int i = 0; i < path.Count; i++)
            {
                Point target = path[i];
                int segDx = target.X - prevPos.X;
                int segDy = target.Y - prevPos.Y;

                // 如果是最后一段并且有 finalStance，用它覆盖
                int stance;
                if (finalStance.HasValue && i == path.Count - 1)
                    stance = finalStance.Value;
                else
                    stance = segDx >= 0 ? 2 : 3;

                var velocity = new Point(
                    (int)((long)Math.Abs(segDx) * 1000 / Math.Max(segmentDurationMs, 1)),
                    (int)((long)Math.Abs(segDy) * 1000 / Math.Max(segmentDurationMs, 1))
                );

                AbsoluteLifeMovement alm = new(0, target, segmentDurationMs, stance);
                alm.setPixelsPerSecond(velocity);
                alm.setFh(GetFoothold(target));
                movements.Add(alm);
                prevPos = target;
            }

            BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(path[^1]);
        }

        // ================================================================
        //  传送移动 —— 注意：不用 TeleportMovement.serialize()
        //  改用 AbsoluteLifeMovement + 极短时间模拟闪现效果
        // ================================================================

        /// <summary>
        /// 闪现到目标位置。
        ///
        /// 正确的 TeleportMovement 通信格式（IDA 确认 case 3/4/7/8/9/11）：
        ///   [cmd:1B][x:i2][y:i2][fh:i2][newstate:1B][duration:i2] = 10B
        ///
        /// ⚠️ 服务端 TeleportMovement.serialize() 输出的是：
        ///   [cmd:1B][x:i2][y:i2][vx:i2][vy:i2][newstate:1B] = 9B（无 fh，无 duration）
        ///   与客户端期望格式不兼容，直接使用会导致接收端解析错位。
        ///
        /// 替代方案：用 AbsoluteLifeMovement(cmd=0) + duration=10ms 达到"瞬移"效果，
        /// 这样服务端 serialize 和客户端 Encode 完全一致。
        /// fh 自动从坐标推导。
        /// </summary>
        /// <param name="pos">目标位置</param>
        /// <param name="newstate">到达后的姿态，默认 0（站立朝右）</param>
        public void TeleportTo(Point pos, int newstate = 0)
        {
            var startPos = getPosition();

            // 用 cmd=0 + 极短 duration 模拟闪现，避免 TeleportMovement 的格式兼容问题
            List<LifeMovementFragment> movements =
            [
                new AbsoluteLifeMovement(0, pos, 10, newstate)
            ];
            ((AbsoluteLifeMovement)movements[0]).setPixelsPerSecond(new Point(0, 0));
            ((AbsoluteLifeMovement)movements[0]).setFh(GetFoothold(pos));

            BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(pos);
        }

        // ================================================================
        //  爬绳子 —— 两步走：走到绳子底部 → 爬上去
        // ================================================================


        /// <summary>
        /// 广播空闲姿态。假人停止移动后应定期广播此包，使角色正确"站"在地面上。
        ///
        /// GetIdleMovementBytes() 继承自 AbstractAnimatedMapObject，生成：
        ///   [numCommands:1B=1][cmd:0(绝对移动)][x:2B][y:2B][vx:0][vy:0][fh:0]
        ///   [stance:1B][duration:0]
        /// 共 13 字节，表示"我在当前位置以姿态 X 站立"。
        /// </summary>
        public void BroadcastIdle(int? newstate = null)
        {
            if (newstate.HasValue)
            {
                setStance(newstate.Value);
            }
            BroadcastMovement(PacketCreator.MovePlayerIdle(Id, GetIdleMovementBytes()), getPosition());
        }

        // ================================================================
        //  跳跃 —— 使用 RelativeLifeMovement（Cmd 1）
        //  兼容性警告：见下方注释
        // ================================================================

        /// <summary>
        /// 跳跃/短位移 —— 使用 RelativeLifeMovement（Cmd 1）。
        ///
        /// 通信格式（IDA 确认 case 1/2/6/12/13/16/18/19/20/22）：
        ///   [cmd:1B][xoff:i2][yoff:i2][newstate:1B][duration:i2] = 8B
        ///
        /// 客户端 Decode 把通信中的 xoff/yoff 写入 ELEM.vx/vy 字段，
        /// 而位置字段 x/y 从上一个元素复制。客户端 Encode 输出的是 vx/vy。
        ///
        /// ⚠️ 服务端 RelativeLifeMovement.serialize() 输出的是位置字段 x/y，
        ///    而不是 vx/vy。8 字节长度一致，但语义不同：
        ///      服务端输出：位置 x/y → 客户端读到 vx/vy 中
        ///      客户端期望：相对偏移到 vx/vy
        ///    建议优先使用 WalkTo/WalkPath（cmd=0），完全无兼容问题。
        /// </summary>
        [Obsolete("优先使用 WalkTo/WalkPath（cmd=0，格式完全兼容），本文的 cmd=1 有字段语义差异")]
        public void Jump(Point relativeDisplacement, int durationMs = 300, int newstate = 4)
        {
            var startPos = getPosition();
            Point targetPos = new(startPos.X + relativeDisplacement.X, startPos.Y + relativeDisplacement.Y);

            List<LifeMovementFragment> movements =
            [
                new RelativeLifeMovement(1, targetPos, durationMs, newstate)
            ];

            BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(targetPos);
        }


        // ================================================================
        //  自动跟随 —— 尝试走到玩家位置，失败则传送
        // ================================================================

        private int _followFailCount;
        private Point? _followLastTarget;

        /// <summary>
        /// 跟随主人到当前所在位置。
        ///
        /// 策略（渐进 + 兜底）：
        ///   第 1 次：WalkToStepped 水平走到主人 X 坐标 → 若主人是攀爬姿态则 ClimbTo
        ///   第 2 次：再试一次（可能上次走到一半被阻挡）
        ///   第 3 次：TeleportTo 直接闪现
        ///   >3 次：返回 false，调用方自行销毁重建
        ///
        /// 每次 Follow 成功后计数清零；目标位置变化时计数也清零。
        /// </summary>
        /// <param name="closeEnoughPx">认为"到达"的距离阈值（像素）</param>
        /// <returns>true=到达目标, false=需销毁重建</returns>
        public bool Follow(Player owner, int closeEnoughPx = 60)
        {
            Point targetPos = owner.getPosition();
            int targetStance = owner.getStance();

            // 目标变了 → 重置计数
            if (_followLastTarget == null || Distance(targetPos, _followLastTarget.Value) > closeEnoughPx)
            {
                _followFailCount = 0;
                _followLastTarget = targetPos;
            }

            // 尝试移动
            bool reached = TryFollowMove(targetPos, targetStance, closeEnoughPx);

            if (reached)
            {
                _followFailCount = 0;
                return true;
            }

            _followFailCount++;

            if (_followFailCount <= 2)
            {
                // 再来一次
                reached = TryFollowMove(targetPos, targetStance, closeEnoughPx);
                if (reached)
                {
                    _followFailCount = 0;
                    return true;
                }
                _followFailCount++;
            }

            if (_followFailCount <= 3)
            {
                // 第三次——闪现
                setStance(targetStance);
                TeleportTo(targetPos);
                if (Distance(getPosition(), targetPos) <= closeEnoughPx)
                {
                    _followFailCount = 0;
                    return true;
                }
                _followFailCount++;
            }

            // 超过 3 次失败 → 需要调用方销毁重建
            return false;
        }

        /// <summary>
        /// 单次跟随尝试。不做寻路——直接走直线，到不了就返回 false，
        /// 由 Follow 的重试/闪现兜底。
        /// </summary>
        private bool TryFollowMove(Point targetPos, int targetStance, int closeEnoughPx)
        {
            var startPos = getPosition();
            double dist = Distance(startPos, targetPos);

            if (dist <= closeEnoughPx)
            {
                setStance(targetStance);
                return true;
            }

            // 直接 WalkToStepped 走直线到目标位置。
            // 如果跨越不同平台/在空中的部分，客户端会渲染为行走 + 下落动画，
            // 看起来不太自然，但 Follow 会快速重试 → TeleportTo 兜底。
            setStance(targetStance);
            WalkToStepped(targetPos);

            return Distance(getPosition(), targetPos) <= closeEnoughPx;
        }

        private static double Distance(Point a, Point b)
        {
            int dx = a.X - b.X;
            int dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
