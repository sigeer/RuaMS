using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Shared.Constants;
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
    ///   2/3 走  4/5. 站 6/7. 跳 16/17. 爬绳子   朝右/左
    /// </summary>
    public class FakePlayer : Player
    {
        public FakePlayer(Player owner, IMap map, Point pos, int idx)
            : base(
                new FakeClient(owner.Client.CurrentServer),
                map,
                pos,
                new PlayerGetterDto
                {
                    Character = new Dto.CharacterDto
                    {
                        Sp = "0,0,0,0,0,0",
                        Hp = NumericConfig.MaxHP,
                        Maxhp = NumericConfig.MaxHP,
                        Mp = NumericConfig.MaxMP,
                        Maxmp = NumericConfig.MaxMP,
                    }
                }
            )
        {
            Id = GetFakePlayerId(owner, idx);
            Name = $"$假人{idx}号$";
            Face = owner.Face;
            Hair = owner.Hair;
            Gender = owner.Gender;
            Level = owner.Level;
            JobModel = owner.JobModel;

            Party = -1;
            setStance(owner.getStance());
            Client.SetPlayer(this);
        }

        public override Player? Controller => this;
        public override int getObjectId()
        {
            return Id;
        }

        static int GetFakePlayerId(Player chr, int idx)
        {
            return 500000 + chr.Id * 100 + idx;
        }

        protected override bool IsVisibleForPlayerWithoutRange(Player chr)
        {
            return true;
        }

        public override Task OnTick(long now)
        {
            // TODO: 在此实现巡逻/跟随等定时行为
            return Task.CompletedTask;
        }

        public override ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        AbsoluteLifeMovement GenereateFinalMovements(Point startPos, Point finalPos)
        {
            var finalFh = MapModel.FindFh(finalPos);
            var finalStance = (int)(finalFh >= 0 ? StanceConstant.StandR : StanceConstant.ClimbR);
            if (startPos.X > finalPos.X)
            {
                // 向左
                finalStance++;
            }
            AbsoluteLifeMovement final = new(0, finalPos, 0, finalStance);
            final.setFh(finalFh);
            return final;
        }

        #region Walk
        /// <summary>
        /// 分段行走 —— 将长距离切分成多步，每段用 AbsoluteLifeMovement 连续输出。
        ///
        /// 每段根据实际段距和 speedPxPerSec 自动计算持续时间，保证客户端
        /// 不论段距大小都以恒定速度 px/s 插值。
        ///
        /// 每段的 fh 自动从终点坐标推导。
        /// </summary>
        /// <param name="targetPos">目标位置</param>
        /// <param name="stepPx">每段最大像素</param>
        /// <param name="speedPxPerSec">目标移动速度（像素/秒），默认 125 ≈ 基础行走速度</param>
        public async Task WalkToStepped(Point targetPos, int stepPx = 120, int speedPxPerSec = 125)
        {
            var startPos = getPosition();
            int dx = targetPos.X - startPos.X;
            int dy = targetPos.Y - startPos.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= stepPx)
            {
                await WalkPath([targetPos], speedPxPerSec);
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

            await WalkPath(path, speedPxPerSec);
        }

        // ================================================================
        //  路径行走 —— 多段 AbsoluteLifeMovement 单包广播
        //  格式同上，每段独立设定位置/速度/姿态
        // ================================================================

        /// <summary>
        /// 沿路径行走 —— 把多个 AbsoluteLifeMovement 拼在一个包里一次性广播。
        /// 这是客户端 CMovePath 的典型用法，每条指令依次执行形成连贯行走。
        ///
        /// 每段根据实际段距和 speedPxPerSec 自动计算持续时间，让客户端
        /// 无论段距大小都以恒定速度 px/s 插值。
        ///
        /// 每段的 fh 自动从该段终点坐标推导。
        /// </summary>
        /// <param name="path">路径点列表（至少一个点）</param>
        /// <param name="speedPxPerSec">目标移动速度（像素/秒）</param>
        /// <param name="finalStance">最后一段终点的姿态，null=自动根据方向决定</param>
        async Task WalkPath(List<Point> path, int speedPxPerSec = 125)
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
                double segDist = Math.Sqrt(segDx * segDx + segDy * segDy);
                int segDurationMs = Math.Max(50, (int)(segDist / speedPxPerSec * 1000));

                var fh = MapModel.FindFh(target);
                int newState = (int)(fh >= 0 ? StanceConstant.WalkR : StanceConstant.ClimbR);
                if (segDx < 0)
                    newState += 1;

                var velocity = new Point(
                    (int)((long)Math.Abs(segDx) * 1000 / Math.Max(segDurationMs, 1)),
                    (int)((long)Math.Abs(segDy) * 1000 / Math.Max(segDurationMs, 1))
                );

                AbsoluteLifeMovement alm = new(0, target, segDurationMs, newState);
                alm.setPixelsPerSecond(velocity);
                alm.setFh(fh);
                movements.Add(alm);
                prevPos = target;
            }
            movements.Add(GenereateFinalMovements(startPos, movements[^1].getPosition()));

            await BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(path[^1]);
        }
        #endregion


        #region Jump
        /// <summary>
        /// 水平跳跃 —— 用 AbsoluteLifeMovement（Cmd 0）+ 姿态 6/7。
        /// 格式兼容性同 WalkTo，完全一致 ✓
        ///
        /// 用于两个平台间水平距离较短的跳越。
        /// </summary>
        /// <param name="targetPos">落点位置</param>
        /// <param name="durationMs">跳跃持续时间（毫秒）</param>
        public async Task JumpTo(Point targetPos, int durationMs = 250)
        {
            var startPos = getPosition();
            int dx = targetPos.X - startPos.X;
            int dy = targetPos.Y - startPos.Y;

            if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
            {
                await BroadcastIdle();
                return;
            }

            // 跳跃姿态：6=朝右跳，7=朝左跳
            int newstate = (int)(dx >= 0 ? StanceConstant.JumpR : StanceConstant.JumpL);
            var fh = MapModel.FindFh(targetPos);

            var velocity = new Point(
                (int)((long)Math.Abs(dx) * 1000 / Math.Max(durationMs, 1)),
                (int)((long)Math.Abs(dy) * 1000 / Math.Max(durationMs, 1))
            );

            List<LifeMovementFragment> movements =
            [
                new AbsoluteLifeMovement(0, targetPos, durationMs, newstate),
                GenereateFinalMovements(startPos, targetPos)
            ];
            ((AbsoluteLifeMovement)movements[0]).setPixelsPerSecond(velocity);
            ((AbsoluteLifeMovement)movements[0]).setFh(fh);

            await BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(targetPos);
        }

        /// <summary>
        /// 下落跳跃 —— 用 JumpDownMovement（Cmd 15）。
        /// 通信格式：[cmd:1B][x:i2][y:i2][vx:i2][vy:i2][fh:i2][fh_origin:i2][newstate:1B][duration:i2] = 16B
        /// IDA 确认：服务端 JumpDownMovement.serialize() 与客户端 case 15 完全一致 ✓
        ///
        /// 用于从高处平台跳到低处平台。
        /// </summary>
        /// <param name="targetPos">落点位置</param>
        /// <param name="durationMs">下落持续时间（毫秒）</param>
        public async Task JumpDownTo(Point targetPos, int durationMs = 300)
        {
            var startPos = getPosition();
            int dx = targetPos.X - startPos.X;
            int dy = targetPos.Y - startPos.Y;

            if (Math.Abs(dx) < 1 && Math.Abs(dy) < 1)
            {
                await BroadcastIdle();
                return;
            }

            // 下落姿态：6=朝右跳，7=朝左跳
            int newstate = (int)(dx >= 0 ? StanceConstant.JumpR : StanceConstant.JumpL);

            var velocity = new Point(
                (int)((long)Math.Abs(dx) * 1000 / Math.Max(durationMs, 1)),
                (int)((long)Math.Abs(dy) * 1000 / Math.Max(durationMs, 1))
            );

            int fromFh = MapModel.FindFh(startPos);
            int toFh = MapModel.FindFh(targetPos);

            List<LifeMovementFragment> movements =
            [
                new JumpDownMovement(15, targetPos, durationMs, newstate),
                GenereateFinalMovements(startPos, targetPos)
            ];
            ((JumpDownMovement)movements[0]).setPixelsPerSecond(velocity);
            ((JumpDownMovement)movements[0]).setFh(toFh);
            ((JumpDownMovement)movements[0]).setOriginFh(fromFh);

            await BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(targetPos);
        }
        #endregion

        #region Climb
        /// <summary>
        /// 沿梯子/绳子攀爬到目标位置（调用方应确保目标在同一梯子上）。
        ///
        /// 攀爬用 AbsoluteLifeMovement（Cmd 0），姿态 16（面朝右攀爬）。
        /// 每段根据段距和 speedPxPerSec 自动计算持续时间。
        ///
        /// </summary>
        /// <param name="targetPos">目标位置（应在同一梯子上）</param>
        /// <param name="speedPxPerSec">攀爬速度（像素/秒），默认 80</param>
        private async Task ClimbTo(Point targetPos, int speedPxPerSec = 80)
        {
            var startPos = getPosition();
            int dy = targetPos.Y - startPos.Y;

            if (Math.Abs(dy) < 5)
            {
                setPosition(targetPos);
                await BroadcastIdle();
                return;
            }

            int absDy = Math.Abs(dy);
            int stepPx = 80;

            if (absDy <= stepPx)
            {
                await ClimbPath([targetPos], speedPxPerSec);
            }
            else
            {
                int steps = (int)Math.Ceiling((double)absDy / stepPx);
                var path = new List<Point>(steps);
                double stepY = (double)dy / steps;

                for (int i = 1; i <= steps; i++)
                {
                    path.Add(new Point(
                        targetPos.X,
                        startPos.Y + (int)(stepY * i)
                    ));
                }

                await ClimbPath(path, speedPxPerSec);
            }
        }

        /// <summary>
        /// 沿路径攀爬 —— 把多个 AbsoluteLifeMovement 拼在一个包里一次性广播。
        /// </summary>
        /// <param name="fh">梯子的落脚点是负数</param>
        /// <param name="path">攀爬路径点列表</param>
        /// <param name="speedPxPerSec">攀爬速度（像素/秒）</param>
        private async Task ClimbPath(List<Point> path, int speedPxPerSec = 80)
        {
            if (path.Count == 0) return;

            var startPos = getPosition();
            var movements = new List<LifeMovementFragment>();
            Point prevPos = startPos;

            for (int i = 0; i < path.Count; i++)
            {
                Point target = path[i];
                int segDy = target.Y - prevPos.Y;
                int segDurationMs = Math.Max(50, (int)((double)Math.Abs(segDy) / speedPxPerSec * 1000));

                var velocity = new Point(0, (int)((long)Math.Abs(segDy) * 1000 / Math.Max(segDurationMs, 1)));

                AbsoluteLifeMovement alm = new(0, target, segDurationMs, 16);
                alm.setPixelsPerSecond(velocity);
                alm.setFh(MapModel.FindFh(target));
                movements.Add(alm);
                prevPos = target;
            }

            movements.Add(GenereateFinalMovements(startPos, movements[^1].getPosition()));

            await BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, movements), startPos);
            setPosition(path[^1]);
        }
        #endregion


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
        public async Task TeleportTo(Point pos)
        {
            var startPos = getPosition();
            await BroadcastMovement(PacketCreator.MovePlayer(Id, startPos, [GenereateFinalMovements(startPos, pos)]), startPos);
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
        public async Task BroadcastIdle(int? newstate = null)
        {
            if (newstate.HasValue)
            {
                setStance(newstate.Value);
            }
            await BroadcastMovement(PacketCreator.MovePlayerIdle(Id, GetIdleMovementBytes()), getPosition());
        }

        /// <summary>
        /// 从当前位置移动到目标位置，支持跨平台、跨梯子的多段导航。
        ///
        /// 策略：
        ///   - 同一梯子 → 直接 ClimbTo
        ///   - 同一平台（Foothold） → 直接 WalkToStepped
        ///   - 不同平台/梯子 → 用 MapNavigator 构建导航图，BFS 寻路，
        ///     自动处理：爬下梯子 → 步行 → 爬上梯子 → 步行 → ... → 到达
        ///   - 无路径可达 → TeleportTo 兜底
        /// </summary>
        public async Task Move(Point target)
        {
            var currentPos = getPosition();
            // Long-distance warp: 如果目标超出视野范围（≈一个屏幕），直接传送
            double rangedDistSq = Client.CurrentServer.NodeService.ServerConfig.SystemConfig.GetRangedDistance();
            int dx = target.X - currentPos.X;
            int dy = target.Y - currentPos.Y;
            if (dx * dx + dy * dy > rangedDistSq)
            {
                await TeleportTo(target);
                return;
            }

            var currentFh = MapModel.FindFh(currentPos);
            var targetFh = MapModel.FindFh(target);
            if (currentFh == targetFh)
            {
                if (currentFh < 0)
                {
                    // 同一个梯子/绳子
                    await ClimbTo(target);
                    return;
                }
                else
                {
                    // 同一个落脚点
                    await WalkToStepped(target);
                    return;
                }
            }

            // Complex path: use navigation graph (BFS over platform-ladder graph)
            var navigator = GetOrCreateNavigator();
            var path = navigator.FindPath(currentPos, target);

            if (path == null || path.Count == 0)
            {
                await TeleportTo(target);
                return;
            }

            foreach (var action in path)
            {
                switch (action.Type)
                {
                    case MoveActionType.Walk:
                        await WalkToStepped(action.To);
                        break;
                    case MoveActionType.Climb:
                        await ClimbTo(action.To);
                        break;
                    case MoveActionType.Jump:
                        int deltaY = action.To.Y - getPosition().Y;
                        if (deltaY > 30) // 向下跳 >30px → 用 JumpDownMovement
                            await JumpDownTo(action.To);
                        else
                            await JumpTo(action.To);
                        break;
                }
            }
        }

        private MapNavigator GetOrCreateNavigator()
        {
            return MapNavigator.GetOrCreate(MapModel.SourceTemplate, MapModel.Id);
        }
    }
}
